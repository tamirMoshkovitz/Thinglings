using UnityEngine;
using System.Collections.Generic;
using _SLIME.BaseScripts;
using _SLIME.Boss;

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

    [Header("Trigger Condition")]
    [Tooltip("How many attacks until the sequence starts.")]
    [SerializeField] private int attacksToStart = 5;

    // State Machine Flags
    private bool _conditionMet;
    private bool _isFading;          // Step 1
    private bool _isRampingMovement; // Step 2

    // Timers
    private float _fadeTimer;
    private float _movementTimer;

    // Cache for Reset
    private float _initialSpeed;
    private float _initialSensitivityX;
    private float _initialSensitivityY;
    
    // Fade Cache
    private SpriteRenderer[] _fadeRenderers;
    private readonly Dictionary<SpriteRenderer, float> _initialAlphas = new Dictionary<SpriteRenderer, float>();

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

    private void Update()
    {
        // Check triggers
        if (!_conditionMet)
        {
            _conditionMet = BossBaseBehaviour.TotalAttacksPreformed >= attacksToStart;
            if (_conditionMet)
            {
                StartFadeSequence();
            }
        }

        // Handle Sequence
        if (_isFading)
        {
            HandleFadeTransition();
        }
        else if (_isRampingMovement)
        {
            HandleMovementTransition();
        }
    }

    private void StartFadeSequence()
    {
        // Step 1: Start Fade
        if (layerToFade != null && _fadeRenderers != null && _fadeRenderers.Length > 0)
        {
            _isFading = true;
            _fadeTimer = 0f;
        }
        else
        {
            // If there is nothing to fade, skip directly to movement
            StartMovementSequence();
        }
    }

    private void StartMovementSequence()
    {
        // Step 2: Start Movement Ramp
        _isRampingMovement = true;
        _movementTimer = 0f;
        
        // Capture start values at the moment the ramp begins
        if (artConfigurations != null)
        {
            _initialSpeed = artConfigurations.tunnelMovementSettings.movementSpeed;
            _initialSensitivityX = artConfigurations.parallaxSettings.sensitivityMultiplierX;
            _initialSensitivityY = artConfigurations.parallaxSettings.sensitivityMultiplierY;
        }
    }

    private void HandleFadeTransition()
    {
        _fadeTimer += Time.deltaTime;
        float progress = Mathf.Clamp01(_fadeTimer / fadeDuration);
        
        float curveValue = fadeCurve.Evaluate(progress);

        if (_fadeRenderers != null)
        {
            foreach (var sr in _fadeRenderers)
            {
                if (sr == null || !_initialAlphas.ContainsKey(sr)) continue;

                float startAlpha = _initialAlphas[sr];
                float newAlpha = Mathf.Lerp(startAlpha, targetAlpha, curveValue);
                
                Color c = sr.color;
                c.a = newAlpha;
                sr.color = c;
            }
        }

        // When fade is done, trigger the next step
        if (progress >= 1f)
        {
            _isFading = false;
            StartMovementSequence();
        }
    }

    private void HandleMovementTransition()
    {
        _movementTimer += Time.deltaTime;
        float progress = Mathf.Clamp01(_movementTimer / transitionDuration);

        float curveValue = accelerationCurve.Evaluate(progress);

        float newSpeed = Mathf.Lerp(_initialSpeed, targetSpeed, curveValue);
        
        // Note: Preserving your negative logic from previous requests
        float newSensitivityX = Mathf.Lerp(-_initialSensitivityX, targetSensitivityX, curveValue);
        float newSensitivityY = Mathf.Lerp(-_initialSensitivityY, targetSensitivityY, curveValue);
        
        if (artConfigurations != null)
        {
            artConfigurations.tunnelMovementSettings.movementSpeed = newSpeed;
            artConfigurations.parallaxSettings.sensitivityMultiplierX = -newSensitivityX;
            artConfigurations.parallaxSettings.sensitivityMultiplierY = -newSensitivityY;
        }

        if (progress >= 1f)
        {
            _isRampingMovement = false;
        }
    }
    
    public void ResetPhase()
    {
        _conditionMet = false;
        _isFading = false;
        _isRampingMovement = false;
        _fadeTimer = 0f;
        _movementTimer = 0f;
        
        ResetSpeed();
    }
    
    private void OnApplicationQuit()
    {
        ResetSpeed();
    }
    
    private void OnDestroy()
    {
        ResetSpeed();
    }

    private void ResetSpeed()
    {
        // 1. Reset Configurations
        if (artConfigurations != null)
        {
            artConfigurations.tunnelMovementSettings.movementSpeed = 0f;
            artConfigurations.parallaxSettings.sensitivityMultiplierX = -0.1f;
            artConfigurations.parallaxSettings.sensitivityMultiplierY = -0.1f;
        }

        // 2. Reset Faded Layer Opacity
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
}