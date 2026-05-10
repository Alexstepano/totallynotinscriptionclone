using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class CardController : MonoBehaviour
{
    public CardsDataSO Data { get; private set; }
    public int CurrentHP { get; private set; }
    public bool IsInHand { get; private set; }
    public CardSlot CurrentSlot { get; private set; }

    private MeshFilter _mf;
    private MeshRenderer _mr;

    public void Initialize(CardsDataSO data)
    {
        Data = data;
        CurrentHP = data.cardHp;
        IsInHand = true;

        _mf = GetComponent<MeshFilter>();
        _mr = GetComponent<MeshRenderer>();

        if (_mf && data.cardMesh) _mf.sharedMesh = data.cardMesh;
        if (_mr && data.cardMaterial) _mr.sharedMaterial = data.cardMaterial;
    }

    private void SpawnEffect(GameObject effectPrefab)
    {
        if (!effectPrefab) return;
        var effect = Instantiate(effectPrefab, transform.position, transform.rotation);
        var ps = effect.GetComponent<ParticleSystem>();
        Destroy(effect, ps ? ps.main.duration + 0.1f : 2f);
    }

    public void MoveToSlot(CardSlot slot)
    {
        if (!slot || !slot.spawnPoint) return;
        CurrentSlot = slot;
        IsInHand = false;

        transform.SetParent(slot.transform, worldPositionStays: true);
        transform.SetPositionAndRotation(slot.spawnPoint.position, slot.spawnPoint.rotation);

        SpawnEffect(Data.playEffectPrefab);
    }

    public void Die()
    {
        SpawnEffect(Data.deathEffectPrefab);
        CurrentSlot?.OnCardDied(this);
        Destroy(gameObject, 0.5f);
    }

    public void PlayAbility() => SpawnEffect(Data.abilityEffectPrefab);

    public void TakeDamage(int amount)
    {
        CurrentHP -= amount;
        if (CurrentHP <= 0) Die();
    }

    // Клик по карте
    /*private void OnMouseDown()
    {
        if (!IsInHand) return;
        Debug.Log($"[Card] Clicked: {Data.cardName}");
        HandController.Instance?.SelectCard(this);
    }*/
}