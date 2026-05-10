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
        yield return new WaitForSeconds(0.5f);

        if (BoardManager.Instance == null) { Debug.LogError("[Game] BoardManager missing!"); yield break; }

        var pSlots = BoardManager.Instance.GetPlayerSlots();
        var eSlots = BoardManager.Instance.GetEnemySlots();

        if (pSlots == null || eSlots == null) yield break;

        int maxSlots = Mathf.Min(pSlots.Count, eSlots.Count);

        for (int i = 0; i < maxSlots; i++)
        {
            var pCard = pSlots[i]?.currentCard;
            var eCard = eSlots[i]?.currentCard;

            if (pCard != null && eCard != null)
            {
                eCard.TakeDamage(pCard.Data.cardAttack);
                pCard.TakeDamage(eCard.Data.cardAttack);
            }
            else if (pCard != null)
            {
                Debug.Log($"[Combat] {pCard.Data.cardName} hits Enemy directly!");
            }
        }

        yield return new WaitForSeconds(0.8f);
        StartCoroutine(EnemyTurnRoutine());
    }

    private IEnumerator EnemyTurnRoutine()
    {
        yield return new WaitForSeconds(1f);
        Debug.Log("[Game] Enemy turn started.");


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
        DrawPlayerCard();
        Debug.Log("[Game] === PLAYER TURN ===");
    }
}