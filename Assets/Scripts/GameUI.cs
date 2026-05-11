using UnityEngine;
using TMPro;

public class GameUI : MonoBehaviour
{
    public static GameUI Instance { get; private set; }

    [Header("Status Bars")]
    public TextMeshProUGUI playerHealthText;
    public TextMeshProUGUI enemyHealthText;
    public TextMeshProUGUI energyText;

    [Header("Game Info")]
    public TextMeshProUGUI turnInfoText;

    private void Awake()
    {
        if (Instance && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }


    public void UpdateUI()
    {
        if (GameController.Instance == null) return;


        if (playerHealthText) playerHealthText.text = $"HP: {GameController.Instance.playerHealth}";
        if (enemyHealthText) enemyHealthText.text = $"Enemy HP: {GameController.Instance.enemyHealth}";


        if (energyText) energyText.text = $"{GameController.Instance.playerEnergy} / {GameController.Instance.maxEnergy}";


        if (turnInfoText)
        {
            turnInfoText.text = GameController.Instance.isPlayerTurn ? "YOUR TURN" : " ENEMY TURN";
            turnInfoText.color = GameController.Instance.isPlayerTurn ? Color.green : Color.red;
        }
    }


    public void OnEndTurnButton()
    {
        if (GameController.Instance != null)
        {
            GameController.Instance.EndPlayerTurn();
        }
    }
}