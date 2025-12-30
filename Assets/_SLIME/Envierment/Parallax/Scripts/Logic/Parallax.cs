using System;
using System.Collections.Generic;
using UnityEngine;

public enum ParallaxLayers
{
    Layer1,
    Layer2,
    Layer3,
    Layer4,
    Layer5,
    Boss
}
public class Parallax : MonoBehaviour
{
    [Header("Player Transforms")]
    [SerializeField] private List<Transform> players;
    
    [Header("Art Configurations")]
    [SerializeField] private ArtConfigurations artConfigurations;
    
    [Header("Parallax Layer Selection")]
    [SerializeField] private ParallaxLayers parallaxLayer;

    private Vector3 _startObjPos;
    private Vector2 _startPlayersPos;
    
    private float _currentVelocityX;
    private float _currentVelocityY;
    
    private ArtConfigurations.ParallaxSettings _currentSettings;

    private void Start()
    {
        _startObjPos = transform.position;
        _startPlayersPos = GetPlayersCenter();
        _currentSettings = artConfigurations.GetSettings(parallaxLayer);
    }

    private void LateUpdate()
    {
        if (players == null || players.Count == 0) return;

        Vector2 currentPlayersPos = GetPlayersCenter();
        Vector2 playerDistMoved = currentPlayersPos - _startPlayersPos;

        float rawOffsetX = playerDistMoved.x * _currentSettings.sensitivityX;
        float clampedOffsetX = Mathf.Clamp(rawOffsetX, -_currentSettings.maxShiftX, _currentSettings.maxShiftX);
        float targetX = _startObjPos.x + clampedOffsetX;

        float rawOffsetY = playerDistMoved.y * _currentSettings.sensitivityY;
        float clampedOffsetY = Mathf.Clamp(rawOffsetY, -_currentSettings.maxShiftY, _currentSettings.maxShiftY);
        float targetY = _startObjPos.y + clampedOffsetY;

        float newX = Mathf.SmoothDamp(transform.position.x, targetX, ref _currentVelocityX, _currentSettings.smoothing);
        float newY = Mathf.SmoothDamp(transform.position.y, targetY, ref _currentVelocityY, _currentSettings.smoothing);

        transform.position = new Vector3(newX, newY, transform.position.z);
    }

    private Vector2 GetPlayersCenter()
    {
        if (players.Count == 0) return Vector2.zero;

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
        if (artConfigurations == null) return;
        var settings = Application.isPlaying ? _currentSettings : artConfigurations.GetSettings(parallaxLayer);
        if (settings == null) return;

        Vector3 center = Application.isPlaying ? _startObjPos : transform.position;
        
        Gizmos.color = new Color(1, 0.92f, 0.016f, 0.4f);
        
        Vector3 size = new Vector3(settings.maxShiftX * 2, settings.maxShiftY * 2, 0.1f);
        Gizmos.DrawCube(center, size);
        
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(center, size);
    }
}