using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;
[RequireComponent(typeof(BoxCollider))]
public class CardController : MonoBehaviour
{
    private int turnsOnBoard = 0;
    public CardsDataSO Data { get; private set; }
    public int CurrentHP { get; private set; }
    public bool IsInHand { get; private set; }
    public CardSlot CurrentSlot { get; private set; }

    [Header("UI Stats")]
    public TextMeshPro hpText;
    public TextMeshPro attackText;

    public TextMeshPro costText;

    [Header("Visual Feedback")]

    public float damageNumberLifetime = 1.2f;
    public Color damageColor = Color.red;
    public Color healColor = Color.green;
    private MeshFilter _mf;
    private MeshRenderer _mr;
    private Material _originalMaterial;
    private AudioSource _audioSource;
    private CameraShake _cameraShake;

    public void Initialize(CardsDataSO data)
    {
        Data = data;
        CurrentHP = data.cardHp;
        IsInHand = true;

        _mf = GetComponent<MeshFilter>();
        _mr = GetComponent<MeshRenderer>();
        _audioSource = GetComponent<AudioSource>();

        if (_mf && data.cardMesh) _mf.sharedMesh = data.cardMesh;
        if (_mr && data.cardMaterial)
        {
            _originalMaterial = data.cardMaterial;
            _mr.sharedMaterial = data.cardMaterial;
        }



        if (_cameraShake == null)
            _cameraShake = FindAnyObjectByType<CameraShake>();

        RefreshStatsUI();

    }

    private void SpawnEffect(GameObject effectPrefab, Vector3? offset = null)
    {
        if (!effectPrefab) return;
        Vector3 pos = offset ?? transform.position;
        var effect = Instantiate(effectPrefab, transform.position, transform.rotation);
        var ps = effect.GetComponent<ParticleSystem>();
        Destroy(effect, ps ? ps.main.duration + 0.1f : 2f);
    }


    public void RefreshStatsUI()
    {
        if (hpText) hpText.text = CurrentHP.ToString();
        if (attackText) attackText.text = Data.cardAttack.ToString();
        if (costText) costText.text = Data.cardCost.ToString();
    }

    public void MoveToSlot(CardSlot slot)
    {
        if (!slot || !slot.spawnPoint) return;
        CurrentSlot = slot;
        IsInHand = false;

        transform.SetParent(slot.transform, worldPositionStays: true);
        transform.SetPositionAndRotation(slot.spawnPoint.position, slot.spawnPoint.rotation);

        SpawnEffect(Data.playEffectPrefab);
        PlaySound(Data.playSound);
    }

    public void Die()
    {
        SpawnEffect(Data.deathEffectPrefab);
        PlaySound(Data.deathSound);



        if (Data.property == CardProperty.SummonOnDeath && Data.summonOnDeathData != null)
        {

            if (CurrentSlot != null)
            {
                CurrentSlot.currentCard = null;
            }
            TriggerSummon();
        }

        CurrentSlot?.OnCardDied(this);
        Destroy(gameObject, 0.5f);
    }

    public void PlayAbility()
    {
        SpawnEffect(Data.abilityEffectPrefab);
        PlaySound(Data.abilitySound);
    }

    public void TakeDamage(int amount)
    {
        CurrentHP -= amount;
        Debug.Log($"[{Data.cardName}] HP: {CurrentHP}");

        SpawnEffect(Data.damageEffectPrefab, transform.position + Vector3.up * 0.3f);
        RefreshStatsUI();
        if (_mr) StartCoroutine(FlashRed());
        PlaySound(Data.damageSound);


        if (amount >= 4 && _cameraShake != null)
            _cameraShake.Shake(0.15f, 0.3f);


        if (CurrentHP <= 0) Die();

    }

    private void PlaySound(AudioClip clip)
    {
        if (!clip) return;
        if (!_audioSource) _audioSource = gameObject.AddComponent<AudioSource>();
        _audioSource.PlayOneShot(clip);
    }


    public void Highlight(bool active)
    {
        if (!_mr) return;
        if (active && Data.selectedMaterial != null)
            _mr.material = Data.selectedMaterial;
        else if (!active)
            _mr.material = _originalMaterial;

        if (active) SpawnEffect(Data.selectEffectPrefab);
    }


    private IEnumerator FlashRed()
    {
        Color original = _mr.material.color;
        _mr.material.color = Color.red;
        yield return new WaitForSeconds(0.1f);
        _mr.material.color = original;
    }


    private void TriggerSummon()
    {
        if (!BoardManager.Instance || !CurrentSlot) return;

        bool isPlayer = BoardManager.Instance.IsPlayerSlot(CurrentSlot);


        var slots = isPlayer ? BoardManager.Instance.GetPlayerSlots() : BoardManager.Instance.GetEnemySlots();
        int slotIndex = -1;
        for (int i = 0; i < slots.Count; i++)
        {
            if (slots[i] == CurrentSlot) { slotIndex = i; break; }
        }

        if (slotIndex >= 0)
        {
            BoardManager.Instance.SummonCardInSlot(Data.summonOnDeathData, slotIndex, isPlayer);
        }
    }


    public void OnBoardTurnStart()
    {
        if (IsInHand || CurrentSlot == null || Data.property != CardProperty.GrowOlder || Data.summonOnDeathData == null) return;

        turnsOnBoard++;
        if (turnsOnBoard >= Data.turnsToGrow)
        {
            Evolve();
        }
    }


    private void Evolve()
    {
        if (Data.summonOnDeathData == null) return;


        CardsDataSO previousForm = Data;
        Data = Data.summonOnDeathData;
        CurrentHP = Data.cardHp;

        if (_mf && Data.cardMesh) _mf.sharedMesh = Data.cardMesh;
        if (_mr && Data.cardMaterial) _mr.sharedMaterial = Data.cardMaterial;


        RefreshStatsUI();


        SpawnEffect(Data.abilityEffectPrefab);
        PlaySound(Data.abilitySound);

        Debug.Log($"[Evolve] {previousForm.cardName} -> {Data.cardName}");



    }

}