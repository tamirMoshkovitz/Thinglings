using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using _SLIME.BaseScripts;
using _SLIME.Boss;
using _SLIME.GameLoop;

public class TempPhaseChanger : MonoBehaviour
{
    [Header("Configurations")]
    [SerializeField] private ArtConfigurations artConfigurations;
    
    [Header("Phase 1: Fade Out Settings")]
    [Tooltip("The GameObject (parent or single) you want to fade out first.")]
    [SerializeField] private GameObject layerToFade;
    [Tooltip("Time in seconds to complete the fade.")]
    [SerializeField] private float fadeDuration = 3f;
    [Tooltip("Curve for opacity (0 = Initial Alpha, 1 = Target Alpha).")]
    [SerializeField] private AnimationCurve fadeCurve = AnimationCurve.Linear(0, 0, 1, 1);
    [Tooltip("Final alpha value (0 = Invisible, 1 = Opaque).")]
    [SerializeField] private float targetAlpha = 0f;

    [Header("Phase 2: Movement Ramp Settings")]
    [Tooltip("The target speed to reach during the ramp-up (After fade is done).")]
    [SerializeField] private float targetSpeed = 50f;
    [Tooltip("How long the ramp-up transition takes in seconds.")]
    [SerializeField] private float transitionDuration = 5f;
    [Tooltip("Curve to control the rate of acceleration (0 to 1).")]
    [SerializeField] private AnimationCurve accelerationCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    [Tooltip("Target sensitivity X")]
    [SerializeField] private float targetSensitivityX = 1f;
    [Tooltip("Target sensitivity Y")]
    [SerializeField] private float targetSensitivityY = 1f;
    
    [Header("Icicle Spawner")]
    [SerializeField] IcicleSpawner spawner;
    
    [Header("Creatures Manager")]
    [SerializeField] private GameObject creatureManager;
    
    [Header("Phase Specific GameObject Lists")]
    [SerializeField] private List<GameObject> objectsToActivateInSecondPhase = new List<GameObject>();
    [SerializeField] private List<GameObject> objectsToActivateInTunnelPhase = new List<GameObject>();

    // Sequence Control
    private Coroutine _phaseSequenceCoroutine;
    private SpriteRenderer[] _fadeRenderers;
    private readonly Dictionary<SpriteRenderer, float> _initialAlphas = new Dictionary<SpriteRenderer, float>();

    private void OnEnable()
    {
        SecondPhaseState.SecondPhaseStarted += OnSecondPhaseStarted;
        TunnelPhaseState.TunnelPhaseStarted += OnTunnelPhaseStarted;
        GameEvents.SlimeWon += OnSlimeWon;
    }
    
    private void OnDisable()
    {
        SecondPhaseState.SecondPhaseStarted -= OnSecondPhaseStarted;
        TunnelPhaseState.TunnelPhaseStarted -= OnTunnelPhaseStarted;
        GameEvents.SlimeWon -= OnSlimeWon;
        
        // Safety: Reset values when script is disabled to prevent SO data corruption
        ResetToDefaultState();
    }

    private void Start()
    {
        if (layerToFade != null)
        {
            _fadeRenderers = layerToFade.GetComponentsInChildren<SpriteRenderer>();
            foreach (var sr in _fadeRenderers)
            {
                if (sr != null && !_initialAlphas.ContainsKey(sr))
                {
                    _initialAlphas.Add(sr, sr.color.a);
                }
            }
        }
    }

    private void OnSecondPhaseStarted()
    {
        if(creatureManager) creatureManager.SetActive(true);
        foreach (var obj in objectsToActivateInSecondPhase)
        {
            if (obj) obj.SetActive(true);
        }
    }

    private void OnTunnelPhaseStarted()
    {
        foreach (var obj in objectsToActivateInTunnelPhase)
        {
            if (obj) obj.SetActive(true);
        }

        if (_phaseSequenceCoroutine != null) StopCoroutine(_phaseSequenceCoroutine);
        _phaseSequenceCoroutine = StartCoroutine(TunnelPhaseSequence());
    }

    private IEnumerator TunnelPhaseSequence()
    {
        // --- STEP 1: FADE TRANSITION ---
        if (layerToFade && _fadeRenderers != null && _fadeRenderers.Length > 0)
        {
            float timer = 0f;
            while (timer < fadeDuration)
            {
                timer += Time.deltaTime;
                float progress = Mathf.Clamp01(timer / fadeDuration);
                float curveValue = fadeCurve.Evaluate(progress);

                foreach (var sr in _fadeRenderers)
                {
                    if (!sr || !_initialAlphas.ContainsKey(sr)) continue;
                    float startAlpha = _initialAlphas[sr];
                    Color c = sr.color;
                    c.a = Mathf.Lerp(startAlpha, targetAlpha, curveValue);
                    sr.color = c;
                }
                yield return null;
            }
            layerToFade.SetActive(false);
        }

        if (artConfigurations != null)
        {
            float timer = 0f;
            float startSpeed = artConfigurations.tunnelMovementSettings.movementSpeed;
            float startX = artConfigurations.parallaxSettings.sensitivityMultiplierX;
            float startY = artConfigurations.parallaxSettings.sensitivityMultiplierY;

            while (timer < transitionDuration)
            {
                timer += Time.deltaTime;
                float progress = Mathf.Clamp01(timer / transitionDuration);
                float curveValue = accelerationCurve.Evaluate(progress);

                // Lerp Speed
                artConfigurations.tunnelMovementSettings.movementSpeed = Mathf.Lerp(startSpeed, targetSpeed, curveValue);
                
                // Lerp Sensitivities (Directly to target)
                artConfigurations.parallaxSettings.sensitivityMultiplierX = Mathf.Lerp(startX, -targetSensitivityX, curveValue);
                artConfigurations.parallaxSettings.sensitivityMultiplierY = Mathf.Lerp(startY, -targetSensitivityY, curveValue);
                
                yield return null;
            }
        }
        
        _phaseSequenceCoroutine = null;
    }

    private void OnSlimeWon()
    {
        if (_phaseSequenceCoroutine != null) StopCoroutine(_phaseSequenceCoroutine);
        StartCoroutine(StopTunnelMovement());
    }

    private IEnumerator StopTunnelMovement()
    {
        float timer = 0f;
        float currentSpeed = artConfigurations.tunnelMovementSettings.movementSpeed;

        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            float progress = Mathf.Clamp01(timer / fadeDuration);
            artConfigurations.tunnelMovementSettings.movementSpeed = Mathf.Lerp(currentSpeed, 0f, fadeCurve.Evaluate(progress));
            yield return null;
        }
        
        artConfigurations.parallaxSettings.sensitivityMultiplierX = -0.1f;
        artConfigurations.parallaxSettings.sensitivityMultiplierY = -0.1f;
        if(creatureManager) creatureManager.SetActive(false);
    }

    private void ResetToDefaultState()
    {
        if (artConfigurations != null)
        {
            artConfigurations.tunnelMovementSettings.movementSpeed = 0f;
            artConfigurations.parallaxSettings.sensitivityMultiplierX = -0.1f;
            artConfigurations.parallaxSettings.sensitivityMultiplierY = -0.1f;
        }

        if (_fadeRenderers != null)
        {
            foreach (var sr in _fadeRenderers)
            {
                if (sr != null && _initialAlphas.ContainsKey(sr))
                {
                    Color c = sr.color;
                    c.a = _initialAlphas[sr];
                    sr.color = c;
                }
            }
        }
    }

    private void OnApplicationQuit() => ResetToDefaultState();
    private void OnDestroy() => ResetToDefaultState();
}