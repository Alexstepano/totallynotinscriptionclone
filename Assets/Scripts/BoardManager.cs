using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BoardManager : MonoBehaviour
{
    public static BoardManager Instance { get; private set; }
    [SerializeField] public List<CardSlot> playerSlots;
    [SerializeField] public List<CardSlot> enemySlots;


    private void Awake()
    {
        if (Instance && Instance != this) { Destroy(gameObject); return; }
        Instance = this;


        if (playerSlots == null) playerSlots = new List<CardSlot>(4);
        if (enemySlots == null) enemySlots = new List<CardSlot>(4);


        Debug.Log($"[Board] Player slots: {playerSlots.Count}, Enemy slots: {enemySlots.Count}");

    }



    private bool TryPlaceCardInternal(CardController card, int slotIndex, List<CardSlot> slots)
    {
        Debug.Log($"[Board] Attempting to place {card.Data.cardName} on   slot {slotIndex}.");
        if (card == null || slots == null)
        {
            Debug.LogError($"[Board] FAILED: Card or Slots array is null.");
            return false;
        }
        if (slotIndex < 0 || slotIndex >= slots.Count)
        {

            Debug.LogWarning($"[Board] FAILED: Slot index {slotIndex} out of range (valid: 0-{slots.Count - 1}).");
            return false;
        }

        var slot = slots[slotIndex];
        if (!slot) return false;
        if (slot.IsOccupied)
        {
            Debug.LogWarning($"[Board] FAILED: Slot {slotIndex}  is already occupied by {slot.currentCard.Data.cardName}.");
            return false;

        }
        card.MoveToSlot(slot);
        slot.currentCard = card;
        Debug.Log($"[Board] SUCCESS: {card.Data.cardName} placed on   slot {slotIndex}.");
        return true;
    }



    public bool TryPlacePlayerCard(CardController card, int slotIndex)
    {
        return TryPlaceCardInternal(card, slotIndex, playerSlots);
    }


    public bool TryPlaceEnemyCard(CardController card, int slotIndex)
    {
        return TryPlaceCardInternal(card, slotIndex, enemySlots);
    }


    public IReadOnlyList<CardSlot> GetEnemySlots()
    {

        return enemySlots ?? new List<CardSlot>();
    }

    public IReadOnlyList<CardSlot> GetPlayerSlots()
    {
        return playerSlots ?? new List<CardSlot>();
    }


    public int GetSlotIndex(CardSlot targetSlot)
    {
        if (playerSlots != null)
        {
            for (int i = 0; i < playerSlots.Count; i++)
            {
                if (playerSlots[i] == targetSlot) return i;
            }
        }

        return -1;
    }



    public CardController SummonCardInSlot(CardsDataSO data, int slotIndex, bool isPlayerSide)
    {
        var slots = isPlayerSide ? playerSlots : enemySlots;
        if (slotIndex < 0 || slotIndex >= slots.Count) return null;

        var targetSlot = slots[slotIndex];
        if (targetSlot == null || targetSlot.IsOccupied) return null;


        if (HandController.Instance == null || HandController.Instance.cardPrefab == null) return null;

        GameObject go = Instantiate(HandController.Instance.cardPrefab, Vector3.zero, Quaternion.identity);
        CardController newCard = go.GetComponent<CardController>();
        if (!newCard)
        {
            Destroy(go);
            Debug.Log($"[Summon]Error: {data.cardName}  HAVE NOT summoned in slot {slotIndex}");
            return null;
        }

        newCard.Initialize(data);


        newCard.MoveToSlot(targetSlot);
        targetSlot.currentCard = newCard;

        Debug.Log($"[Summon] {data.cardName} summoned in slot {slotIndex}");
        return newCard;
    }


    public bool IsPlayerSlot(CardSlot slot) => playerSlots.Contains(slot);
}
