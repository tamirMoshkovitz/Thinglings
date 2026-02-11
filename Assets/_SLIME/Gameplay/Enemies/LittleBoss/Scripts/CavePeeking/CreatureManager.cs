using System.Collections;
using System.Collections.Generic;
using _SLIME.Boss; // Ensure this namespace is active
using UnityEngine;
using Random = UnityEngine.Random;

public class CreatureManager : MonoBehaviour
{
    [Header("References")]
    public BossBrain bossBrain; 

    [Header("Pool of Creatures")]
    public List<PeekingCreature> allCreatures = new List<PeekingCreature>();

    [Header("Atmospheric Depth (Color by Distance)")]
    public int farSortingOrder = 0;   
    public int closeSortingOrder = 20; 
    public Color farColor = new Color(0.4f, 0.4f, 0.4f, 1f); 
    public Color closeColor = Color.white; 
    [Range(0f, 1f)] public float hiddenDimFactor = 0.3f; 

    [Header("Timing Ranges")]
    public Vector2 spawnDelayRange = new Vector2(1f, 4f); 
    public Vector2 phaseTwoEndTargetRange = new Vector2(0.5f, 1f);
    
    [Header("Progression")]
    [Tooltip("X axis = Health Lost (0 to 1), Y axis = Interpolation (0 to 1).")]
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
        _initialSpawnDelayRange = spawnDelayRange;
        _nextSpawnTime = Time.time + Random.Range(spawnDelayRange.x, spawnDelayRange.y);

        // Calculate colors immediately on start
        ApplyDepthColors();
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
        // 1. Update the difficulty (Spawn Rate) dynamically
        UpdateSpawnRateBasedOnHealth();

        // 2. Handle Spawning
        if (Time.time >= _nextSpawnTime)
        {
            TryTriggerCreature();
            
            // Pick next time based on CURRENT spawnDelayRange (which might have changed)
            float delay = Random.Range(spawnDelayRange.x, spawnDelayRange.y);
            _nextSpawnTime = Time.time + delay;
        }
    }

    /// <summary>
    /// Linearly interpolates the spawn delay based on Boss Health.
    /// </summary>
    void UpdateSpawnRateBasedOnHealth()
    {
        // Stop updating based on HP if we are in the tunnel phase (scripted event)
        if (_inTunnelPhase) return;

        // Safety checks
        if (!bossBrain || BossBrain.bossConfigurations == null) return;

        // Get Health Data
        float maxHp = BossBrain.bossConfigurations.CoreSettings.maxHealth;
        float lowerThreshold = BossBrain.bossConfigurations.PhaseSettings.lowerHealthThreshold;
        float currentHp = bossBrain.currentHealth;

        // Calculate progress (0 = Full HP, 1 = Threshold HP)
        // InverseLerp handles the fact that HP is decreasing (High to Low)
        float healthProgress = Mathf.InverseLerp(maxHp, lowerThreshold, currentHp);

        // Evaluate curve (allows for non-linear difficulty spikes)
        float curvedProgress = difficultyCurve.Evaluate(healthProgress);

        // Smoothly adjust the spawn delay range
        spawnDelayRange = Vector2.Lerp(_initialSpawnDelayRange, phaseTwoEndTargetRange, curvedProgress);
    }

    [ContextMenu("Apply Depth Colors")]
    public void ApplyDepthColors()
    {
        if (allCreatures == null) return;

        foreach (var creature in allCreatures)
        {
            if (creature == null) continue;

            int order = creature.GetTargetSortingOrder();
            float t = Mathf.InverseLerp(farSortingOrder, closeSortingOrder, order);

            Color myVisibleColor = Color.Lerp(farColor, closeColor, t);
            Color myHiddenColor = myVisibleColor * hiddenDimFactor;
            myHiddenColor.a = 1f; 

            creature.visibleColor = myVisibleColor;
            creature.hiddenColor = myHiddenColor;
        }
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

    private void OnPhaseChangeToTunnel()
    {
        _inTunnelPhase = true;
        // Stop any existing tween to avoid conflict
        StopAllCoroutines(); 
        StartCoroutine(LerpSpawnRate(tunnelPhaseTransitionDuration, tunnelPhaseTargetRange));
    }

    private IEnumerator LerpSpawnRate(float duration, Vector2 targetRange)
    {
        Vector2 startRange = spawnDelayRange;
        float elapsed = 0;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            // Ease out for smoother transition
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
        ApplyDepthColors(); 
    }
}