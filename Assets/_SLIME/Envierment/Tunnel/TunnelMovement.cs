using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using _SLIME.BaseScripts;

public class TunnelMovement : ProjectMonoBehavior
{
    [Header("Configuration")]
    [Tooltip("The scriptable object containing global tunnel settings.")]
    [SerializeField] private ArtConfigurations artConfigurations;

    private ArtConfigurations.TunnelMovementSettings _settings;
    private List<LayerData> _layers;
    
    private float _globalScrollPosition;

    private class LayerData
    {
        public Transform Transform;
        public SpriteRenderer Renderer;
        public Color OriginalColor;
        public float BaseOffset;
    }

    private void OnEnable()
    {
        _settings = artConfigurations.tunnelMovementSettings;
        _globalScrollPosition = 0f;

        InitializeLayers();
        UpdateTunnelLogic(0f);
    }

    private void Update()
    {
        UpdateTunnelLogic(Time.deltaTime);
    }

    private void UpdateTunnelLogic(float deltaTime)
    {
        float perspectiveAlpha = CalculatePerspective();
        UpdateScrollPosition(deltaTime);
        ApplyToLayers(perspectiveAlpha);
    }

    private void InitializeLayers()
    {
        _layers = new List<LayerData>();

        var validChildren = new List<Transform>();
        for (int i = 0; i < transform.childCount; i++)
        {
            Transform child = transform.GetChild(i);
            if (child.GetComponent<SpriteRenderer>() != null && child.gameObject.activeSelf)
            {
                validChildren.Add(child);
            }
        }

        validChildren = validChildren.OrderBy(t => t.GetSiblingIndex()).ToList();

        int validCount = validChildren.Count;

        for (int i = 0; i < validCount; i++)
        {
            Transform child = validChildren[i];
            SpriteRenderer sr = child.GetComponent<SpriteRenderer>();
            
            sr.color = Color.white; 

            LayerData data = new LayerData
            {
                Transform = child,
                Renderer = sr,
                OriginalColor = Color.white,
                BaseOffset = (float)i / validCount 
            };
            
            _layers.Add(data);
        }
    }

    private float CalculatePerspective()
    {
        if (!_settings.linkPerspectiveToSpeed) return 0f;

        float speedRatio = Mathf.Clamp01(Mathf.Abs(_settings.movementSpeed) / _settings.maxEffectSpeed);
        return Mathf.Lerp(0f, _settings.targetPerspectiveAtMaxSpeed, speedRatio);
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

    private void ApplyToLayers(float perspAlpha)
    {
        foreach (var layer in _layers)
        {
            float progress = GetLayerProgress(layer);
            
            float zPos = Mathf.Lerp(_settings.respawnPositionZ, _settings.despawnPositionZ, progress);
            
            Vector3 pos = layer.Transform.position;
            pos.z = zPos;
            layer.Transform.position = pos;

            ApplyScale(layer, zPos, perspAlpha);
            ApplyVisuals(layer, progress);
        }
    }

    private float GetLayerProgress(LayerData layer)
    {
        float currentProgress = layer.BaseOffset + _globalScrollPosition;
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

    private void ApplyVisuals(LayerData layer, float progress)
    {
        layer.Renderer.sortingOrder = 100 - (int)(progress * 100);
        
        float alpha = Mathf.Clamp01(_settings.opacityCurve.Evaluate(progress));
        
        if (!_settings.fadeInAtEntrance && progress < 0.2f) 
        {
            alpha = 1f;
        }

        if (!_settings.fadeOutAtExit && progress > 0.8f) 
        {
            alpha = 1f;
        }

        float brightness = 1f;
        if (_settings.useDepthDarkening)
        {
            brightness = Mathf.Clamp01(_settings.brightnessCurve.Evaluate(progress));
        }

        Color targetColor = new Color(
            layer.OriginalColor.r * brightness,
            layer.OriginalColor.g * brightness,
            layer.OriginalColor.b * brightness,
            alpha
        );

        layer.Renderer.color = targetColor;
    }

    private void OnDrawGizmos()
    {
        var tunnelSetting = artConfigurations.tunnelMovementSettings;
        
        Gizmos.color = Color.green;
        Vector3 startPos = new Vector3(transform.position.x, transform.position.y, tunnelSetting.respawnPositionZ);
        Gizmos.DrawWireCube(startPos, Vector3.one * 1f);

        Gizmos.color = Color.red;
        Vector3 endPos = new Vector3(transform.position.x, transform.position.y, tunnelSetting.despawnPositionZ);
        Gizmos.DrawWireCube(endPos, Vector3.one * 1f);
        
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(startPos, endPos);
    }
}