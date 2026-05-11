using UnityEngine;

[CreateAssetMenu(fileName = "CardsDataSO", menuName = "Assets/SO/CardsDataSO.cs")]
public class CardsDataSO : ScriptableObject
{
    public string cardName;
    public int cardCost;
    public int cardAttack;
    public int cardHp;

    [Header("Properties")]
    public CardProperty property = CardProperty.None;

    public CardsDataSO summonOnDeathData;

    public int turnsToGrow = 1;



    [Header("Visual part")]
    public Mesh cardMesh;
    public Material cardMaterial;
    public Material selectedMaterial;

    [Header("Particle Effects")]
    public GameObject playEffectPrefab;
    public GameObject deathEffectPrefab;
    public GameObject abilityEffectPrefab;
    public GameObject damageEffectPrefab;
    public GameObject selectEffectPrefab;

    [Header("Audio")]
    public AudioClip playSound;
    public AudioClip deathSound;
    public AudioClip abilitySound;
    public AudioClip damageSound;

}
public enum CardProperty
{
    None,
    Lifesteal,  // Лечит владельца при ударе - не существо
    Venom,
    Taunt,       // Заставляет атаковать себя
    SummonOnDeath,

    GrowOlder
}