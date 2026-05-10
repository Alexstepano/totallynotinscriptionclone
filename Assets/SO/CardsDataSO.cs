using UnityEngine;

[CreateAssetMenu(fileName = "CardsDataSO", menuName = "Assets/SO/CardsDataSO.cs")]
public class CardsDataSO : ScriptableObject
{
    public string cardName;
    public int cardCost;
    public int cardAttack;
    public int cardHp;


    //card effects there TODO
    [Header("Visual part")]
    public Mesh cardMesh;
    public Material cardMaterial;
    public GameObject playEffectPrefab;
    public GameObject deathEffectPrefab;
    public GameObject abilityEffectPrefab;

}
