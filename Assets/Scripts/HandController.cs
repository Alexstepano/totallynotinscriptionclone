using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class HandController : MonoBehaviour
{
    public static HandController Instance { get; private set; }

    [Header("Setup")]
    public Transform handRoot;
    public Transform[] handPositions;
    public GameObject cardPrefab;

    [Header("Animation")]
    public float layoutSpeed = 6f;
    public float selectLift = 0.25f;

    private readonly List<CardController> cardsInHand = new();
    public int HandCount => cardsInHand.Count;
    public CardController SelectedCard { get; private set; }
    private Vector3 selectedOriginalPos;

    private void Awake()
    {
        if (Instance && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void AddCard(CardsDataSO data)
    {
        if (!cardPrefab || !handRoot) { Debug.LogError("[Hand] Missing Prefab or Root!"); return; }
        if (handPositions == null || handPositions.Length == 0) { Debug.LogError("[Hand] handPositions empty!"); return; }

        int idx = cardsInHand.Count;
        if (idx >= handPositions.Length) { Debug.LogWarning("[Hand] Hand is full!"); return; }


        Transform target = handPositions[idx];
        GameObject go = Instantiate(cardPrefab, target.position, target.rotation, handRoot);

        CardController card = go.GetComponent<CardController>();
        if (!card) { Destroy(go); return; }

        card.Initialize(data);
        cardsInHand.Add(card);


        StartCoroutine(AnimateToPosition(card, target.localPosition, target.localRotation));
    }

    public void UpdateLayout()
    {
        for (int i = 0; i < cardsInHand.Count; i++)
        {
            CardController card = cardsInHand[i];
            if (!card || card == SelectedCard) continue;
            StartCoroutine(AnimateToPosition(card, handPositions[i].localPosition, handPositions[i].localRotation));
        }
    }

    private IEnumerator AnimateToPosition(CardController card, Vector3 targetPos, Quaternion targetRot)
    {
        float t = 0f;
        Vector3 startPos = card.transform.localPosition;
        Quaternion startRot = card.transform.localRotation;

        while (t < 1f)
        {
            t += Time.deltaTime * layoutSpeed;
            card.transform.localPosition = Vector3.Lerp(startPos, targetPos, t);
            card.transform.localRotation = Quaternion.Slerp(startRot, targetRot, t);
            yield return null;
        }
        card.transform.SetLocalPositionAndRotation(targetPos, targetRot);
    }

    public void SelectCard(CardController card)
    {
        if (SelectedCard == card) { DeselectCard(); return; }
        DeselectCard();

        SelectedCard = card;
        selectedOriginalPos = card.transform.localPosition;
        SelectedCard.Highlight(true);
        StartCoroutine(AnimateToPosition(card, card.transform.localPosition + Vector3.up * selectLift, card.transform.localRotation));
    }

    public void DeselectCard()
    {
        if (!SelectedCard) return;
        SelectedCard.Highlight(false);
        StartCoroutine(AnimateToPosition(SelectedCard, selectedOriginalPos, SelectedCard.transform.localRotation));
        SelectedCard = null;
    }


    public bool PlaySelectedCard(int slotIndex)
    {
        if (SelectedCard == null) return false;
        if (!GameController.Instance.CanAfford(SelectedCard.Data.cardCost))
        {
            Debug.Log($"[Hand] Not enough energy! Need: {SelectedCard.Data.cardCost}");
            return false;
        }


        CardController cardToPlay = SelectedCard;
        cardsInHand.Remove(cardToPlay);
        SelectedCard = null;


        bool placed = BoardManager.Instance.TryPlacePlayerCard(cardToPlay, slotIndex);
        if (placed)
        {
            GameController.Instance.SpendEnergy(cardToPlay.Data.cardCost);
            cardToPlay.Highlight(false);
            UpdateLayout(); // Перестраиваем оставшиеся карты
            return true;
        }

        // Если слот занят - возвращаем  в руку
        cardsInHand.Add(cardToPlay);
        cardToPlay.Highlight(false);
        UpdateLayout();
        Debug.Log($"[Hand] Slot {slotIndex} is occupied or invalid. Card returned.");
        return false;
    }

    public void RemoveCard(CardController card)
    {
        if (cardsInHand.Contains(card)) cardsInHand.Remove(card);
        if (SelectedCard == card) DeselectCard();
        UpdateLayout();
    }
}