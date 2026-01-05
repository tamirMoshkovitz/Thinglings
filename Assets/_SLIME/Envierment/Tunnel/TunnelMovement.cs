using UnityEngine;
using System.Collections.Generic;
using _SLIME.BaseScripts;

public class TunnelMovement : ProjectMonoBehavior
{
    [Header("Configuration")]
    [Tooltip("The scriptable object containing global tunnel settings.")]
    [SerializeField] private ArtConfigurations artConfigurations;

    private const float ManualDistribution = 0f;

    private const float ManualPerspective = 0f;

    private ArtConfigurations.TunnelMovementSettings _settings;
    private List<LayerData> _layers;
    
    private float _globalScrollPosition;
    private float _currentDistributionVal; 
    private struct LayerData
    {
        public Transform Transform;
        public SpriteRenderer Renderer;
        public Color OriginalColor;
        public int OriginalSortOrder; 
        public float ManualOffset; 
        public float EvenOffset;   
    }

    private void OnEnable()
    {
        if (artConfigurations != null)
            _settings = artConfigurations.tunnelMovementSettings;

        InitializeLayers();
        UpdateTunnelLogic(0f);
    }

    private void Update()
    {
        UpdateTunnelLogic(Time.deltaTime);
    }

    private void UpdateTunnelLogic(float deltaTime)
    {
        bool isMoving = Mathf.Abs(_settings.movementSpeed) > 0.05f;

        float distributionAlpha = CalculateDistribution(deltaTime, isMoving);
        float perspectiveAlpha = CalculatePerspective();
        UpdateScrollPosition(deltaTime);

        ApplyToLayers(distributionAlpha, perspectiveAlpha, isMoving);
    }

    private void InitializeLayers()
    {
        _layers = new List<LayerData>();
        _globalScrollPosition = 0f;
        _currentDistributionVal = 0f;

        int childCount = transform.childCount;
        for (int i = 0; i < childCount; i++)
        {
            Transform child = transform.GetChild(i);
            SpriteRenderer sr = child.GetComponent<SpriteRenderer>();

            LayerData data = new LayerData
            {
                Transform = child,
                Renderer = sr,
                OriginalColor = new Color(sr.color.r, sr.color.g, sr.color.b, 1f),
                OriginalSortOrder = sr.sortingOrder,
                EvenOffset = (float)i / childCount
            };
            
            float currentZ = Mathf.Clamp(child.position.z, _settings.respawnPositionZ, _settings.despawnPositionZ);
            data.ManualOffset = Mathf.InverseLerp(_settings.respawnPositionZ, _settings.despawnPositionZ, currentZ);
            _layers.Add(data);
        }
    }

    private float CalculateDistribution(float deltaTime, bool isMoving)
    {
        float target = ManualDistribution;

        if (_settings.autoDistributeOnMove)
        {
            target = isMoving ? 1f : 0f;
        }
        _currentDistributionVal = Mathf.MoveTowards(_currentDistributionVal, target, deltaTime * _settings.distributionTransitionSpeed);
        return _currentDistributionVal;
    }

    private float CalculatePerspective()
    {
        float perspective = ManualPerspective;
        
        if (_settings.linkPerspectiveToSpeed)
        {
            float speedRatio = Mathf.Clamp01(Mathf.Abs(_settings.movementSpeed) / _settings.maxEffectSpeed);
            perspective = Mathf.Lerp(ManualPerspective, _settings.targetPerspectiveAtMaxSpeed, speedRatio);
        }
        return perspective;
    }

    private void UpdateScrollPosition(float deltaTime)
    {
        float totalLength = _settings.despawnPositionZ - _settings.respawnPositionZ;
        if (Mathf.Abs(totalLength) < 0.001f) return;

        float speedStep = (_settings.movementSpeed * deltaTime) / totalLength;
        _globalScrollPosition += speedStep;
        
        _globalScrollPosition %= 1f;
        if (_globalScrollPosition < 0) _globalScrollPosition += 1f;
    }

    private void ApplyToLayers(float distAlpha, float perspAlpha, bool isMoving)
    {
        foreach (var layer in _layers)
        {
            float progress = GetLayerProgress(layer, distAlpha);
            float zPos = Mathf.Lerp(_settings.respawnPositionZ, _settings.despawnPositionZ, progress);
            
            Vector3 pos = layer.Transform.position;
            pos.z = zPos;
            layer.Transform.position = pos;

            ApplyScale(layer, zPos, perspAlpha);
            ApplyVisuals(layer, progress, distAlpha, isMoving);
        }
    }

    private float GetLayerProgress(LayerData layer, float distAlpha)
    {
        float baseOffset = Mathf.Lerp(layer.ManualOffset, layer.EvenOffset, distAlpha);
        float currentProgress = baseOffset + _globalScrollPosition;
        return currentProgress - Mathf.Floor(currentProgress);
    }

    private void ApplyScale(LayerData layer, float zPos, float perspAlpha)
    {
        float safeZ = Mathf.Max(zPos, 0.001f); 
        float full3DScale = (_settings.cameraFocalScale / safeZ) * _settings.baseLayerScale;
        float flatScale = _settings.baseLayerScale;
        float finalScale = Mathf.Lerp(flatScale, full3DScale, perspAlpha);
        
        if (finalScale > 100f) finalScale = 100f; 
        layer.Transform.localScale = Vector3.one * finalScale;
    }

    private void ApplyVisuals(LayerData layer, float progress, float distAlpha, bool isMoving)
    {
        if (isMoving)
        {
            layer.Renderer.sortingOrder = 100 - (int)(progress * 100);
        }
        else
        {
            layer.Renderer.sortingOrder = layer.OriginalSortOrder;
        }

        float curveAlpha = _settings.opacityCurve.Evaluate(progress);

        if (!_settings.fadeInAtEntrance && progress < 0.5f) curveAlpha = 1f;
        if (!_settings.fadeOutAtExit && progress >= 0.5f) curveAlpha = 1f;

        float stoppedAlpha = 1f; 
        float finalAlpha = Mathf.Lerp(stoppedAlpha, curveAlpha, distAlpha);
        float brightness = 1f;
        
        if (_settings.useDepthDarkening)
        {
            brightness = _settings.brightnessCurve.Evaluate(progress);
        }

        layer.Renderer.color = new Color(
            layer.OriginalColor.r * brightness,
            layer.OriginalColor.g * brightness,
            layer.OriginalColor.b * brightness,
            finalAlpha
        );
    }
}