using System.Collections.Generic;
using UnityEngine;

public enum ParallaxLayers
{
    Layer1,
    Layer2,
    Layer3,
    Layer4,
    Layer5
}

public class Parallax : MonoBehaviour
{
    [Header("Player Transforms")]
    [SerializeField] private List<Transform> players;
    
    [Header("Art Configurations")]
    [Tooltip("Reference to the ArtConfigurations ScriptableObject")]
    [SerializeField] private ArtConfigurations artConfigurations;
    
    [Header("Parallax Layer Selection")]
    [Tooltip("Select which parallax layer this object belongs to")]
    [SerializeField] private ParallaxLayers parallaxLayer;

    private float _startObjX;
    private float _startPlayersX;
    private float _currentVelocity;
    private ArtConfigurations.ParallaxSettings _currentSettings;

    private void Start()
    {
        _startObjX = transform.position.x;
        _startPlayersX = GetPlayersCenterX();
        _currentSettings = artConfigurations.GetSettings(parallaxLayer);
    }

    private void LateUpdate()
    {
        if (players == null || players.Count == 0) return;

        float currentPlayersX = GetPlayersCenterX();
        
        float playerDistMoved = currentPlayersX - _startPlayersX;

        float rawOffset = playerDistMoved * _currentSettings.sensitivity;
        
        float clampedOffset = Mathf.Clamp(rawOffset, -_currentSettings.maxShift, _currentSettings.maxShift);

        float targetX = _startObjX + clampedOffset;

        float newX = Mathf.SmoothDamp(transform.position.x, targetX, ref _currentVelocity, _currentSettings.smoothing);

        transform.position = new Vector3(newX, transform.position.y, transform.position.z);
    }

    private float GetPlayersCenterX()
    {
        if (players.Count == 0) return 0f;

        float totalX = 0f;
        int activeCount = 0;

        foreach (var p in players)
        {
            if (!p || !p.gameObject.activeSelf) continue;
            totalX += p.position.x;
            activeCount++;
        }
        return activeCount > 0 ? totalX / activeCount : _startPlayersX;
    }
    
    // Uncomment to see boundries
    
    // private void OnDrawGizmosSelected()
    // {
    //     if (!Application.isPlaying) _startObjX = transform.position.x;
    //     
    //     Gizmos.color = Color.yellow;
    //     Gizmos.DrawLine(
    //         new Vector3(_startObjX - _currentSettings.maxShift, transform.position.y, transform.position.z),
    //         new Vector3(_startObjX + _currentSettings.maxShift, transform.position.y, transform.position.z)
    //     );
    // }
}
