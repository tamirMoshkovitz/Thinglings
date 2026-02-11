using System.Collections;
using System.Collections.Generic;
using UnityEngine;
// using _SLIME.Boss; // Uncomment if you have this namespace
using Random = UnityEngine.Random;

public class CreatureManager : MonoBehaviour
{
    [Header("References")]
    // public BossBrain bossBrain; // Uncomment

    [Header("Pool of Creatures")]
    public List<PeekingCreature> allCreatures = new List<PeekingCreature>();

    [Header("Atmospheric Depth (Color by Distance)")]
    public int farSortingOrder = 0;   // Order for background
    public int closeSortingOrder = 20; // Order for foreground
    public Color farColor = new Color(0.4f, 0.4f, 0.4f, 1f); // Darker/Dimmed
    public Color closeColor = Color.white; // Bright/Clear
    [Range(0f, 1f)] public float hiddenDimFactor = 0.3f; // How much darker is the "hidden" state?

    [Header("Timing Ranges")]
    public Vector2 spawnDelayRange = new Vector2(1f, 4f); 
    public Vector2 phaseTwoEndTargetRange = new Vector2(0.5f, 1f);
    
    [Header("Progression")]
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
        // TunnelPhaseState.TunnelPhaseStarted += OnPhaseChangeToTunnel; // Uncomment
    }
    
    private void OnDisable()
    {
        // TunnelPhaseState.TunnelPhaseStarted -= OnPhaseChangeToTunnel; // Uncomment
    }
    
    void Update()
    {
        UpdateSpawnRateBasedOnHealth();

        if (Time.time >= _nextSpawnTime)
        {
            TryTriggerCreature();
            float delay = Random.Range(spawnDelayRange.x, spawnDelayRange.y);
            _nextSpawnTime = Time.time + delay;
        }
    }

    /// <summary>
    /// Calculates colors for all creatures based on their Sorting Order
    /// </summary>
    [ContextMenu("Apply Depth Colors")]
    public void ApplyDepthColors()
    {
        if (allCreatures == null) return;

        foreach (var creature in allCreatures)
        {
            if (creature == null) continue;

            // Ask the creature what its sorting order will be (based on its parent)
            int order = creature.GetTargetSortingOrder();

            // Calculate interpolation value (0 to 1)
            float t = Mathf.InverseLerp(farSortingOrder, closeSortingOrder, order);

            // Determine the "Peak" brightness for this specific creature
            Color myVisibleColor = Color.Lerp(farColor, closeColor, t);

            // Determine the "Hidden" brightness (just a darker version of visible)
            Color myHiddenColor = myVisibleColor * hiddenDimFactor;
            // Ensure alpha is 1 (optional, depends if you want fade-out)
            myHiddenColor.a = 1f; 

            // Assign to creature
            creature.visibleColor = myVisibleColor;
            creature.hiddenColor = myHiddenColor;
        }
    }

    void UpdateSpawnRateBasedOnHealth()
    {
        // ... (Keep your original logic here) ...
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
            spawnDelayRange = Vector2.Lerp(startRange, targetRange, t); // Simplified lerp
            yield return null;
        }
        spawnDelayRange = targetRange;
    }
    
    [ContextMenu("Auto-Fill Creatures")]
    void AutoFill()
    {
        allCreatures.Clear();
        allCreatures.AddRange(FindObjectsOfType<PeekingCreature>(true));
        ApplyDepthColors(); // Preview colors in editor immediately
    }
}