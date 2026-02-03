using System;
using System.Collections.Generic;
using UnityEngine;

public class Parallax : MonoBehaviour
{
    [Header("Player Transforms")]
    [SerializeField] private List<Transform> players;
    
    [Header("Art Configurations")]
    [SerializeField] private ArtConfigurations artConfigurations;
    
    private class ParallaxLayer
    {
        public Transform transform;
        public Vector3 startPos;
        public float velocityX;
        public float velocityY;
        public float baseDepthSensitivity; 
    }

    private readonly List<ParallaxLayer> _layers = new List<ParallaxLayer>();
    private Vector2 _startPlayersPos;
    private ArtConfigurations.ParallaxSettings _currentSettings;

    private void Start()
    {
        _currentSettings = artConfigurations.parallaxSettings;
        _startPlayersPos = GetPlayersCenter();

        InitializeLayers();
    }

    private void InitializeLayers()
    {
        _layers.Clear();

        // 1. Find the Depth Range (Min Z and Max Z) of all children
        float minZ = float.MaxValue;
        float maxZ = float.MinValue;

        foreach (Transform child in transform)
        {
            if (child.position.z < minZ) minZ = child.position.z;
            if (child.position.z > maxZ) maxZ = child.position.z;
        }

        // Avoid division by zero if all objects are at the same Z
        float depthRange = maxZ - minZ;
        if (depthRange < 0.001f) depthRange = 1f;

        foreach (Transform child in transform)
        {
            float zDepth = child.position.z;

            // 2. Normalize Z to a 0-1 range regardless of actual world coordinates
            // This ensures the Curve works even if objects are at Z = 100
            float normalizedDepth = (zDepth - minZ) / depthRange;

            float depthFactor = _currentSettings.depthSensitivityCurve.Evaluate(normalizedDepth);

            ParallaxLayer newLayer = new ParallaxLayer
            {
                transform = child,
                startPos = child.position,
                baseDepthSensitivity = depthFactor,
                velocityX = 0f,
                velocityY = 0f
            };
            
            _layers.Add(newLayer);
        }
    }

private void LateUpdate()
{
    Vector2 currentPlayersPos = GetPlayersCenter();
    Vector2 playerDistMoved = currentPlayersPos - _startPlayersPos;

    // 1. Dynamic Range Check
    // We recalculate the min/max Z every frame because TunnelMovement is 
    // constantly shifting these layers.
    float minZ = float.MaxValue;
    float maxZ = float.MinValue;
    foreach (var l in _layers)
    {
        float z = l.transform.position.z;
        if (z < minZ) minZ = z;
        if (z > maxZ) maxZ = z;
    }
    float depthRange = Mathf.Max(maxZ - minZ, 0.001f);

    foreach (var layer in _layers)
    {
        // 2. Map current Z to the 0-1 range for the curve
        float normalizedDepth = (layer.transform.position.z - minZ) / depthRange;
        
        // 3. Sample the curve (Make sure the curve never hits 0 in the Inspector!)
        float currentFactor = _currentSettings.depthSensitivityCurve.Evaluate(normalizedDepth);

        // 4. Apply multipliers
        float dynamicSensitivityX = currentFactor * _currentSettings.sensitivityMultiplierX;
        float dynamicSensitivityY = currentFactor * _currentSettings.sensitivityMultiplierY;
        
        // 5. Calculate Target with clamping
        float rawOffsetX = playerDistMoved.x * dynamicSensitivityX;
        float targetX = layer.startPos.x + Mathf.Clamp(rawOffsetX, -_currentSettings.maxShiftX, _currentSettings.maxShiftX);
        
        float rawOffsetY = playerDistMoved.y * dynamicSensitivityY;
        float targetY = layer.startPos.y + Mathf.Clamp(rawOffsetY, -_currentSettings.maxShiftY, _currentSettings.maxShiftY);

        // 6. SmoothDamp for fluid movement
        float newX = Mathf.SmoothDamp(layer.transform.position.x, targetX, ref layer.velocityX, _currentSettings.smoothing);
        float newY = Mathf.SmoothDamp(layer.transform.position.y, targetY, ref layer.velocityY, _currentSettings.smoothing);

        if (float.IsNaN(newX) || float.IsNaN(newY)) continue;

        // Apply position while preserving the Z position from TunnelMovement
        layer.transform.position = new Vector3(newX, newY, layer.transform.position.z);
    }
}
    private Vector2 GetPlayersCenter()
    {
        Vector2 totalPos = Vector2.zero;
        int activeCount = 0;

        foreach (var p in players)
        {
            if (!p || !p.gameObject.activeSelf) continue;
            totalPos.x += p.position.x;
            totalPos.y += p.position.y;
            activeCount++;
        }
        return activeCount > 0 ? totalPos / activeCount : _startPlayersPos;
    }

    private void OnDrawGizmosSelected()
    {
        // Safety check to prevent errors in Editor before Play Mode
        var settings = (Application.isPlaying && _currentSettings != null) ? _currentSettings : (artConfigurations != null ? artConfigurations.parallaxSettings : null);
        if (settings == null) return;

        Gizmos.color = new Color(1, 0.92f, 0.016f, 0.2f);
        
        foreach (Transform child in transform)
        {
            Vector3 center = Application.isPlaying && _layers.Count > 0 
                ? GetStartPosForGizmo(child) 
                : child.position;

            Vector3 size = new Vector3(settings.maxShiftX * 2, settings.maxShiftY * 2, 0.1f);
            Gizmos.DrawWireCube(center, size);
        }
    }

    private Vector3 GetStartPosForGizmo(Transform t)
    {
        if (_layers == null) return t.position;
        var layer = _layers.Find(l => l.transform == t);
        return layer != null ? layer.startPos : t.position;
    }
}