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

    [Header("State (Read Only)")]
    [SerializeField] private bool _inTunnelPhase = false;

    private float _nextSpawnTime;
    private Vector2 _initialSpawnDelayRange;
    private Coroutine _activeTransition;

    private void Start()
    {
        _initialSpawnDelayRange = spawnDelayRange;
        _nextSpawnTime = Time.time + Random.Range(spawnDelayRange.x, spawnDelayRange.y);
        
        ApplyDepthColors();

        // FIX: Check if we are ALREADY in the Tunnel Phase because we might have missed the event
        CheckInitialBossState();
    }
    
    private void OnEnable()
    {
        TunnelPhaseState.TunnelPhaseStarted += OnPhaseChangeToTunnel;
    }
    
    private void OnDisable()
    {
        TunnelPhaseState.TunnelPhaseStarted -= OnPhaseChangeToTunnel;
    }

    private void CheckInitialBossState()
    {
        if (bossBrain == null) return;

        // Verify if the current state of the FSM is already the TunnelPhaseState
        // You may need to adjust 'CurrentState' based on your BossBrain implementation
        if (bossBrain.StateMachine.CurrentState is TunnelPhaseState)
        {
            OnPhaseChangeToTunnel();
        }
    }
    
    void Update()
    {
        if (!_inTunnelPhase)
        {
            UpdateSpawnRateBasedOnHealth();
        }

        if (Time.time >= _nextSpawnTime)
        {
            TryTriggerCreature();
            float delay = Random.Range(spawnDelayRange.x, spawnDelayRange.y);
            _nextSpawnTime = Time.time + delay;
        }
    }

    void UpdateSpawnRateBasedOnHealth()
    {
        if (!bossBrain || BossBrain.bossConfigurations == null) return;

        float maxHp = BossBrain.bossConfigurations.CoreSettings.maxHealth;
        float lowerThreshold = BossBrain.bossConfigurations.PhaseSettings.lowerHealthThreshold;
        float currentHp = bossBrain.currentHealth;

        float healthProgress = Mathf.InverseLerp(maxHp, lowerThreshold, currentHp);
        float curvedProgress = difficultyCurve.Evaluate(healthProgress);

        spawnDelayRange = Vector2.Lerp(_initialSpawnDelayRange, phaseTwoEndTargetRange, curvedProgress);
    }

    private void OnPhaseChangeToTunnel()
    {
        if (_inTunnelPhase) return;
        
        _inTunnelPhase = true; 

        if (_activeTransition != null) StopCoroutine(_activeTransition);
        _activeTransition = StartCoroutine(LerpSpawnRate(tunnelPhaseTransitionDuration, tunnelPhaseTargetRange));
    }

    private IEnumerator LerpSpawnRate(float duration, Vector2 targetRange)
    {
        Vector2 startRange = spawnDelayRange;
        float elapsed = 0;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            float smoothT = t * t * (3f - 2f * t); 
        
            spawnDelayRange = Vector2.Lerp(startRange, targetRange, smoothT);
            yield return null;
        }

        spawnDelayRange = targetRange;
        _activeTransition = null;
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
            if (useGlobalHoldTime) selected.peekStayTime = globalHoldTime;
            selected.Peek();
        }
    }

    [ContextMenu("Auto-Fill Creatures")]
    void AutoFill()
    {
        allCreatures.Clear();
        allCreatures.AddRange(FindObjectsOfType<PeekingCreature>(true));
        ApplyDepthColors(); 
    }
}