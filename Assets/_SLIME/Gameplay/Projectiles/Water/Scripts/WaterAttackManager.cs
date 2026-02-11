using System;
using System.Collections;
using System.Collections.Generic;
using _SLIME.Boss;
using _SLIME.Core.Audio.FMOD.Scripts;
using _SLIME.Envierment.Earthquake.Scriptables;
using _SLIME.GameLoop;
using _SLIME.Slime;
using FMODUnity;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class WaterAttackManager : MonoBehaviour
{
    private static readonly int CreaturesInsideTrigger = Animator.StringToHash("Creatures inside");
    private static readonly int AttackModeTrigger = Animator.StringToHash("Attack mode");
    private static readonly int MagicalWaterOutTrigger = Animator.StringToHash("Magical water out");
    private static readonly int SlimeWon = Animator.StringToHash("SlimeWon");
    private static readonly int BossWon = Animator.StringToHash("BossWon");
    private static readonly int IsLastPhase = Animator.StringToHash("Last Phase");

    [Header("Settings")]
    [SerializeField] private float initialDelay = 1f;
    [SerializeField] private float timeToAttackMode = 2f;
    [SerializeField] private float timeToMagicOut = 1f;
    [SerializeField] private GameObject waterSpellPrefab;
    [SerializeField] private float spawnInterval;
    [SerializeField] private Transform rightWaterMark;
    [SerializeField] private Transform leftWaterMark;
    private float waterAttackDamage => BossBrain.bossConfigurations.WaterAttack.waterDamage;

    [Header("References")] [SerializeField]
    private Animator animator;
    [SerializeField] private BossBrain bossBrain;
    
    [Header("Earthquake Effect")]
    [SerializeField] private EarthquakeUtil earthquakeUtil;
    [SerializeField] private Camera camera;
    [SerializeField] private List<GameObject> iciclesParticles;

    [Header("SFX")]
    [SerializeField] private EventReference watterAttackSFX;
    
    private bool _isLeftZoneActive;
    private bool _isRightZoneActive;
    private bool _attacked;
    private int waterAttackResult = BossWon;
    
    private Coroutine _attackRoutine;
    public bool CanAttack { get; private set; }

    public void SetZoneState(int sensorId, bool isActive)
    {
        if (sensorId == 0) _isLeftZoneActive = isActive;
        else if (sensorId == 1) _isRightZoneActive = isActive;

        CheckSynchronization();
    }

    private void OnEnable()
    {
        TunnelPhaseState.BossDead += OnBossDead;
        // if (IsLastWaterAttack())
        animator.SetBool(IsLastPhase, true);
    }
    
    private void OnDisable()
    {
        TunnelPhaseState.BossDead -= OnBossDead;
        _attacked = false;
    }

    private void CheckSynchronization()
    {
        bool bothActive = _isLeftZoneActive && _isRightZoneActive;

        if (bothActive)
        {
            if (_attackRoutine == null)
            {
                _attackRoutine = StartCoroutine(AttackSequence());
            }
        }
    }

    private IEnumerator AttackSequence()
    {
        if (_attacked) yield break;
        _attacked = true;

        StartCoroutine(TriggerEarthquake());
        SFXPlayer.Play(watterAttackSFX);
        Debug.Log("water attack started");

        yield return new WaitForSeconds(initialDelay);

        GameEvents.FmodPhaseFour?.Invoke();

        float slimeToPlaceDuration = .75f;
        SlimeEvents.SlimeInWaterPosition?.Invoke(slimeToPlaceDuration);
        yield return new WaitForSeconds(slimeToPlaceDuration);
        TriggerBoth(CreaturesInsideTrigger);
        yield return AttackModeCoroutine();

        yield return new WaitForSeconds(1f);
        
        bossBrain.animator.SetTrigger(waterAttackResult);

        TriggerBoth(MagicalWaterOutTrigger);

        _attackRoutine = null;
        GameEvents.WaterAttackEnded?.Invoke();
        Debug.Log("Water Attack Ended");
    }

    private void TriggerBoth(int hashId)
    {
        if(animator) animator.SetTrigger(hashId);
        if (hashId != AttackModeTrigger) return;
        bossBrain.ApplyDamage(waterAttackDamage);
    }

    private IEnumerator AttackModeCoroutine()
    {
        TriggerBoth(AttackModeTrigger);
        
        CanAttack = true;

        float timer = 0f;
        float timePassedFromLastRightSpawn = 0f;
        float timePassedFromLastLeftSpawn = spawnInterval / 2f;

        while (timer < TimeToMagicOut)
        {
            timer += Time.deltaTime;
            timePassedFromLastRightSpawn += Time.deltaTime;
            timePassedFromLastLeftSpawn += Time.deltaTime;

            if (timePassedFromLastRightSpawn >= SpawnInterval)
            {
                spawnSpell(rightWaterMark.position);
                timePassedFromLastRightSpawn = 0f;
            }

            if (timePassedFromLastLeftSpawn >= SpawnInterval)
            {
                spawnSpell(leftWaterMark.position);
                timePassedFromLastLeftSpawn = 0f;
            }
            yield return null;
        }
    }

    private void spawnSpell(Vector3 from)
    {
        Vector3 spawnPosition = from + Vector3.up * Random.Range(-.75f, .75f);
        Instantiate(waterSpellPrefab, spawnPosition, Quaternion.identity);
    }

    private IEnumerator TriggerEarthquake()
    {
        yield return earthquakeUtil.EarthquakeCoroutine(camera, null, 0);

        StartCoroutine(ActivateIcicleFall());
    }

    private void OnBossDead()
    {
        waterAttackResult = SlimeWon;
        GameEvents.SlimeWon?.Invoke();
    }

    private float TimeToMagicOut
    {
        get
        {
            switch (BossCheckpointManager.Instance.CurrentSavedPhase)
            {
                case BossPhaseType.FirstPhase:
                    return timeToMagicOut / 3f;
                case BossPhaseType.SecondPhase:
                    return timeToMagicOut / 2f;
                case BossPhaseType.TunnelPhase:
                    return timeToMagicOut / 1.2f;
            }
            return timeToMagicOut;
        }
    }

    private float SpawnInterval
    {
        get
        {
            switch (BossCheckpointManager.Instance.CurrentSavedPhase)
            {
                case BossPhaseType.FirstPhase:
                    return spawnInterval;
                case BossPhaseType.SecondPhase:
                    return spawnInterval / 1.2f;
                case BossPhaseType.TunnelPhase:
                    return spawnInterval / 2f;
            }
            return spawnInterval;
        }
    }

    private IEnumerator ActivateIcicleFall()
    {
        if (BossCheckpointManager.Instance.CurrentSavedPhase == BossPhaseType.TunnelPhase)
            yield break;
        
        for (int i = 0; i < iciclesParticles.Count; i++)
        {
            yield return new WaitForSeconds(Random.Range(0f, 1f));
            iciclesParticles[i].SetActive(true);
        }
    }
}