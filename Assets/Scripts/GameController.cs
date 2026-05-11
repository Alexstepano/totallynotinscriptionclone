using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameController : MonoBehaviour
{
    public static GameController Instance { get; private set; }

    [Header("Decks")]
    public List<CardsDataSO> playerDeck = new List<CardsDataSO>();
    public List<CardsDataSO> enemyDeck = new List<CardsDataSO>();
    public int maxHandSize = 5;

    [Header("Energy")]
    public int playerEnergy = 3;
    public int maxEnergy = 3;

    [Header("State")]
    public bool isPlayerTurn = true;
    [Header("Health")]
    public int playerHealth = 10;
    public int enemyHealth = 10;

    private void Awake()
    {
        if (Instance && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Start() => StartGame();

    public void StartGame()
    {
        playerEnergy = maxEnergy;
        isPlayerTurn = true;
        for (int i = 0; i < 3; i++) DrawPlayerCard();
        Debug.Log("[Game] Game Started.");
    }

    public void DrawPlayerCard()
    {

        if (HandController.Instance == null) { Debug.LogError("[Game] HandController missing!"); return; }
        if (playerDeck.Count == 0 || HandController.Instance.HandCount >= maxHandSize) return;

        int idx = Random.Range(0, playerDeck.Count);
        CardsDataSO cardData = playerDeck[idx];
        playerDeck.RemoveAt(idx);
        HandController.Instance.AddCard(cardData);
    }

    public bool CanAfford(int cost) => playerEnergy >= cost;
    public void SpendEnergy(int amount) => playerEnergy = Mathf.Max(0, playerEnergy - amount);

    public void EndPlayerTurn()
    {
        if (!isPlayerTurn) return;
        isPlayerTurn = false;
        Debug.Log("[Game] Player turn ended. Resolving combat...");
        StartCoroutine(ResolveCombatRoutine());
    }

    private IEnumerator ResolveCombatRoutine()
    {
        Debug.Log("[Combat] Resolving...");
        yield return new WaitForSeconds(0.5f);

        var pSlots = BoardManager.Instance.GetPlayerSlots();
        var eSlots = BoardManager.Instance.GetEnemySlots();

        //   1: Игрок атакует
        for (int i = 0; i < pSlots.Count; i++)
        {
            var slot = pSlots[i];
            if (slot.currentCard == null) continue;
            ProcessAttack(slot.currentCard, eSlots, true, i);
        }

        yield return new WaitForSeconds(0.5f);

        //2: Враг атакует
        for (int i = 0; i < eSlots.Count; i++)
        {
            var slot = eSlots[i];
            if (slot.currentCard == null) continue;
            ProcessAttack(slot.currentCard, pSlots, false, i);
        }

        CheckWinCondition();
        yield return new WaitForSeconds(0.8f);
        StartCoroutine(EnemyTurnRoutine());
    }




    private void ProcessAttack(CardController attacker, IReadOnlyList<CardSlot> defenderSlots, bool isPlayerAttacking, int attackerSlotIndex)
    {
        CardSlot targetSlot = CardPropertySystem.FindTarget(attackerSlotIndex, defenderSlots);
        int baseDmg = attacker.Data.cardAttack;
        int finalDmg;


        if (targetSlot != null)
        {
            finalDmg = CardPropertySystem.CalculateDamage(attacker, baseDmg, false);
            Debug.Log($"[Combat] {attacker.Data.cardName} -> {targetSlot.currentCard.Data.cardName} ({finalDmg} dmg)");
            targetSlot.currentCard.TakeDamage(finalDmg);

        }
        else
        {

            finalDmg = CardPropertySystem.CalculateDamage(attacker, baseDmg, true);
            if (isPlayerAttacking) enemyHealth -= finalDmg;
            else playerHealth -= finalDmg;
            Debug.Log($"[Combat] {attacker.Data.cardName} hits base for {finalDmg}");
        }
        int heal = CardPropertySystem.CalculateLifesteal(attacker, finalDmg);
        // Применение лечения
        if (heal > 0)
        {
            if (isPlayerAttacking) playerHealth += heal;
            else enemyHealth += heal;
            Debug.Log($"[Lifesteal] +{heal} HP");
        }
    }


    private void CheckWinCondition()
    {
        if (enemyHealth <= 0) { Debug.Log("VICTORY!"); Time.timeScale = 0; }
        else if (playerHealth <= 0) { Debug.Log("DEFEAT!"); Time.timeScale = 0; }
    }


    private IEnumerator EnemyTurnRoutine()
    {
        yield return new WaitForSeconds(1f);
        Debug.Log("[Game] Enemy turn started.");

        CheckBoardEvolution(false);


        if (HandController.Instance == null || HandController.Instance.cardPrefab == null)
        {
            Debug.LogError("[Game] CRITICAL: HandController or CardPrefab is NULL! Enemy cannot play.");
        }
        else if (enemyDeck.Count > 0)
        {
            CardsDataSO data = enemyDeck[0];
            enemyDeck.RemoveAt(0);

            if (data == null)
            {
                Debug.LogWarning("[Game] Enemy Deck had an empty slot.");
            }
            else
            {
                // Создаем карту
                GameObject go = Instantiate(HandController.Instance.cardPrefab, Vector3.zero, Quaternion.identity);
                CardController enemyCard = go.GetComponent<CardController>();

                if (enemyCard == null)
                {
                    Debug.LogError("[Game] ERROR: CardPrefab is missing 'CardController' script! Deleting object.");
                    Destroy(go);
                }
                else
                {
                    enemyCard.Initialize(data);

                    // Ищем свободный слот
                    int freeSlot = -1;
                    var eSlots = BoardManager.Instance.GetEnemySlots();
                    if (eSlots != null)
                    {
                        for (int i = 0; i < eSlots.Count; i++)
                        {
                            if (eSlots[i] != null && !eSlots[i].IsOccupied) { freeSlot = i; break; }
                        }
                    }

                    if (freeSlot >= 0)
                    {
                        BoardManager.Instance.TryPlaceEnemyCard(enemyCard, freeSlot);
                    }
                    else
                    {
                        Debug.Log("[AI] No slots available. Card discarded.");
                        Destroy(go);
                    }
                }
            }
        }

        yield return new WaitForSeconds(0.5f);
        Debug.Log("[Game] Enemy turn finished -> Player Turn Start.");
        StartPlayerTurn();
    }

    private void StartPlayerTurn()
    {
        playerEnergy = maxEnergy;
        isPlayerTurn = true;
        CheckBoardEvolution(true);
        DrawPlayerCard();
        Debug.Log("[Game] === PLAYER TURN ===");
    }


    private void CheckBoardEvolution(bool isPlayer)
    {
        if (isPlayer)
        {
            foreach (var slot in BoardManager.Instance.GetPlayerSlots())
                slot.currentCard?.OnBoardTurnStart();
        }
        else
        {
            foreach (var slot in BoardManager.Instance.GetEnemySlots())
                slot.currentCard?.OnBoardTurnStart();
        }
    }
}