using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    [Header("Settings")]
    public Camera mainCamera;
    public LayerMask slotLayerMask;
    public LayerMask cardLayerMask;
    public float raycastDistance = 50f;

    private Mouse _mouse;
    private Keyboard _keyboard;

    void Awake()
    {
        if (mainCamera == null) mainCamera = Camera.main;
        _mouse = Mouse.current;
        _keyboard = Keyboard.current;
    }

    void Update()
    {
        if (_mouse == null) return;

        //поворот камеры
        if (_mouse.leftButton.isPressed)
        {
            float deltaX = _mouse.delta.ReadValue().x;
            if (Mathf.Abs(deltaX) > 1f)
            {
                CameraController.Instance?.RotateCamera(deltaX);
            }
        }



        // Выбор карты в руке
        if (_mouse.leftButton.wasPressedThisFrame)
        {
            Ray ray = mainCamera.ScreenPointToRay(_mouse.position.ReadValue());
            if (Physics.Raycast(ray, out RaycastHit hit, raycastDistance, cardLayerMask))
            {
                var card = hit.collider.GetComponentInParent<CardController>();
                if (card != null && card.IsInHand)
                {
                    HandController.Instance?.SelectCard(card);
                    return;
                }
            }
            HandController.Instance?.DeselectCard();
        }

        //  Розыгрыш карты (ПКМ / Пробел)
        bool playPressed = _mouse.rightButton.wasPressedThisFrame ||
                           (_keyboard != null && _keyboard.spaceKey.wasPressedThisFrame);

        if (playPressed)
        {
            var hand = HandController.Instance;
            if (hand?.SelectedCard == null) return;

            Ray ray = mainCamera.ScreenPointToRay(_mouse.position.ReadValue());

            if (Physics.Raycast(ray, out RaycastHit hit, raycastDistance, slotLayerMask))
            {
                // Ищем скрипт и на текущем объекте, и на родителях
                var targetSlot = hit.collider.GetComponentInParent<CardSlot>();

                Debug.Log($"[Raycast] Hit: {hit.collider.name} | Layer: {LayerMask.LayerToName(hit.collider.gameObject.layer)} | SlotScript: {(targetSlot != null ? "OK" : "NULL")}");

                if (targetSlot == null)
                {
                    Debug.LogWarning("[Input] Луч попал в объект без CardSlot! Проверь иерархию слотов.");
                    return;
                }

                if (targetSlot.IsOccupied)
                {
                    Debug.Log($"[Input] Слот {targetSlot.name} занят картой {targetSlot.currentCard.Data.cardName}.");
                    return;
                }

                int slotIndex = BoardManager.Instance.GetSlotIndex(targetSlot);
                if (slotIndex >= 0)
                {
                    Debug.Log($"[Input] Выкладываю в слот #{slotIndex}");
                    hand.PlaySelectedCard(slotIndex);
                }
                else
                {
                    Debug.Log("[Input] Не удалось найти индекс слота в BoardManager!");
                }
            }
            else
            {
                Debug.Log("[Input] Луч не попал в слой Slot. Проверь LayerMask в Inspector.");
            }
        }

        // Завершение хода
        if (_keyboard != null && _keyboard.eKey.wasPressedThisFrame)
            GameController.Instance?.EndPlayerTurn();
    }
}