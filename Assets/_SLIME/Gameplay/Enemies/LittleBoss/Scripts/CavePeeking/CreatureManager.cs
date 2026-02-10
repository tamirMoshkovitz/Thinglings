using System.Collections;
using System.Collections.Generic;
using _SLIME.Boss;
using UnityEngine;
using Random = UnityEngine.Random;

public class CreatureManager : MonoBehaviour
{
    [Header("References")]
    public BossBrain bossBrain;

    [Header("Pool of Creatures")]
    public List<PeekingCreature> allCreatures = new List<PeekingCreature>();

    [Header("Timing Ranges")]
    public Vector2 spawnDelayRange = new Vector2(1f, 4f); 
    public Vector2 phaseTwoEndTargetRange = new Vector2(0.5f, 1f); // Make sure this is faster than base
    
    [Header("Progression")]
    [Tooltip("X axis = Health Lost (0 to 1), Y axis = Interpolation (0 to 1). Curve up for drastic changes.")]
    public AnimationCurve difficultyCurve = AnimationCurve.Linear(0,0,1,1);
    
    [Header("Tunnel Phase")]
    public Vector2 tunnelPhaseTargetRange = new Vector2(0.5f, 0.5f);
    public float tunnelPhaseTransitionDuration = 7f;
    
    [Header("Settings Override")]
    public bool useGlobalHoldTime = false;
    public float globalHoldTime = 2.0f;

    private float _nextSpawnTime;
    private Vector2 _initialSpawnDelayRange;
    private bool _inTunnelPhase = false;

    private void Start()
    {
        // Store the initial "Slow" range
        _initialSpawnDelayRange = spawnDelayRange;
        
        // Initialize first spawn
        _nextSpawnTime = Time.time + Random.Range(spawnDelayRange.x, spawnDelayRange.y);
    }
    
    private void OnEnable()
    {
        TunnelPhaseState.TunnelPhaseStarted += OnPhaseChangeToTunnel;
    }
    
    private void OnDisable()
    {
        TunnelPhaseState.TunnelPhaseStarted -= OnPhaseChangeToTunnel;
    }
    
    void Update()
    {
        // 1. Calculate the new Range based on HP
        UpdateSpawnRateBasedOnHealth();

        // 2. Spawn Logic
        if (Time.time >= _nextSpawnTime)
        {
            TryTriggerCreature();
            
            // Pick next random time based on the CURRENT (potentially faster) range
            float delay = Random.Range(spawnDelayRange.x, spawnDelayRange.y);
            _nextSpawnTime = Time.time + delay;
        }
        else
        {
            // --- THE FIX ---
            // If the range has drastically shortened, the current _nextSpawnTime might be too far away.
            // Example: We were waiting 4 seconds, but now max wait is 1 second.
            // We clamp the remaining time to the new max.
            
            float pendingWait = _nextSpawnTime - Time.time;
            if (pendingWait > spawnDelayRange.y)
            {
                // Force the spawn to happen sooner
                _nextSpawnTime = Time.time + spawnDelayRange.y;
            }
        }
    }

    void UpdateSpawnRateBasedOnHealth()
    {
        if (_inTunnelPhase) return; // Don't override tunnel logic
        if (bossBrain == null || BossBrain.bossConfigurations == null) return;

        float maxHp = BossBrain.bossConfigurations.CoreSettings.maxHealth;
        float lowerThreshold = BossBrain.bossConfigurations.PhaseSettings.lowerHealthThreshold;
        float currentHp = bossBrain.currentHealth;

        // 0.0 at Max Health, 1.0 at Phase Change
        float healthProgress = Mathf.InverseLerp(maxHp, lowerThreshold, currentHp);

        // Apply Curve for "Drastic" feel
        float curvedProgress = difficultyCurve.Evaluate(healthProgress);

        // Interpolate
        spawnDelayRange = Vector2.Lerp(_initialSpawnDelayRange, phaseTwoEndTargetRange, curvedProgress);
    }

    void TryTriggerCreature()
    {
        if (allCreatures.Count == 0) return;

        List<PeekingCreature> available = allCreatures.FindAll(c => c.gameObject.activeInHierarchy && !c.IsBusy);

        if (available.Count > 0)
        {
            PeekingCreature selected = available[Random.Range(0, available.Count)];
            
            if (useGlobalHoldTime) 
                selected.peekStayTime = globalHoldTime;

            selected.Peek();
        }
    }

    private Coroutine _spawnTweenCoroutine;

    private void OnPhaseChangeToTunnel()
    {
        _inTunnelPhase = true; // Lock the Update loop from changing values
        if (_spawnTweenCoroutine != null) StopCoroutine(_spawnTweenCoroutine);
        _spawnTweenCoroutine = StartCoroutine(LerpSpawnRate(tunnelPhaseTransitionDuration, tunnelPhaseTargetRange));
    }

    private IEnumerator LerpSpawnRate(float duration, Vector2 targetRange)
    {
        Vector2 startRange = spawnDelayRange;
        float elapsed = 0;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            float easedT = 1 - (1 - t) * (1 - t); 
        
            spawnDelayRange = Vector2.Lerp(startRange, targetRange, easedT);
            yield return null;
        }

        spawnDelayRange = targetRange;
    }
    
    [ContextMenu("Auto-Fill Creatures")]
    void AutoFill()
    {
        allCreatures.Clear();
        allCreatures.AddRange(FindObjectsOfType<PeekingCreature>(true));
    }
}