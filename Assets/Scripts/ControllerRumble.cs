using DefaultNamespace;
using UnityEngine;
using UnityEngine.InputSystem;

public class ControllerRumble : MonoBehaviour
{
    [Header("Tear Rumble Settings")]
    [SerializeField] private bool addTearRumble = true;
    [SerializeField] private float tearRumbleDuration = .2f;
    [SerializeField][Range(0, 1)] private float tearRumbleLowFrequency = .2f;
    [SerializeField][Range(0, 1)] private float tearRumbleHighFrequency = 1f;
    
    [Header("Stretch Rumble Settings")]
    [SerializeField] private Transform rightCenter;
    [SerializeField] private Transform leftCenter;
    [SerializeField] private float maxStretch = 20f;
    [SerializeField] private float connectionDistance = 2f;
    [SerializeField][Range(0, 1)] private float stretchRumbleLowFrequency = .5f;
    [SerializeField][Range(0, 1)] private float stretchRumbleHighFrequency = .5f;
    
    private static bool _reachedMaxStretch = true;
    private float _prevLow = 0f;
    private float _prevHigh;
    [SerializeField] private float rumbleChangeThreshold = 0.02f;

    private bool Connected
    {
        get
        {
            if (!_reachedMaxStretch || Vector3.Distance(leftCenter.position, rightCenter.position) < connectionDistance)
            {
                _reachedMaxStretch = false;
                return true;
            }
            return false;
        }
    }
        
    private void OnEnable()
    {
        GameEvents.SlimeTears += OnSlimeTears;
        GameEvents.PauseGame += OnPauseGame;
        GameEvents.ResumeGame += OnResumeGame;
    }

    private void OnDisable()
    {
        GameEvents.SlimeTears -= OnSlimeTears;
        GameEvents.PauseGame -= OnPauseGame;
        GameEvents.ResumeGame -= OnResumeGame;
    }

    private void Update()
    {
        if (_reachedMaxStretch && Connected) return;

        if (Connected)
        {
            // Stretch rumble based on distance between left and right centers
            float distance = Vector3.Distance(leftCenter.position, rightCenter.position);
            if (distance > maxStretch)
            {
                OnSlimeTears();
                return;
            }
            
            float lowFrequency = CalculateStretchRumble(stretchRumbleLowFrequency, distance);
            float highFrequency = CalculateStretchRumble(stretchRumbleHighFrequency, distance);
            if (Mathf.Abs(lowFrequency - _prevLow) > rumbleChangeThreshold ||
                Mathf.Abs(highFrequency - _prevHigh) > rumbleChangeThreshold)
            {
                Gamepad.current?.SetMotorSpeeds(lowFrequency, highFrequency);
                _prevLow = lowFrequency;
                _prevHigh = highFrequency;
            }
            
            if (lowFrequency > 0 || highFrequency > 0)
                Debug.Log("Stretch rumble ACTIVE");
            else
                Debug.Log("Stretch rumble ZERO");
        }
    }

    private void OnDestroy()
    {
        Gamepad.current?.ResetHaptics();
    }

    private void OnResumeGame()
    {
        Gamepad.current?.ResumeHaptics();
    }

    private void TearRumble(float duration, float lowFrequency, float highFrequency)
    {
        if (_reachedMaxStretch) return;

        _reachedMaxStretch = true;
        StopRumble();
        if (addTearRumble)
        {
            Gamepad.current?.SetMotorSpeeds(lowFrequency, highFrequency);
            Invoke(nameof(StopRumble), duration);
        }
    }

    private void StopRumble()
    {
        Gamepad.current?.ResetHaptics();  
        Gamepad.current?.SetMotorSpeeds(0f, 0f);

        _reachedMaxStretch = true;
    }
    private void OnSlimeTears()
    {
        StopRumble();
        _reachedMaxStretch = false;
        TearRumble(tearRumbleDuration, tearRumbleLowFrequency, tearRumbleHighFrequency);
    }

    private void OnPauseGame()
    {
        StopRumble();
    }

    private float CalculateStretchRumble(float baseFrequency, float distance)
    {
        float normalized = Mathf.Clamp01((distance) / maxStretch);
        return (Mathf.Pow(normalized, 2f)) * baseFrequency; ;
    }

    private void OnDrawGizmos()
    {
        Vector3 direction = (leftCenter.position - rightCenter.position).normalized;
        Vector3 endPoint = rightCenter.position + direction * connectionDistance;

        Debug.DrawLine(
            rightCenter.position,
            endPoint,
            Color.green
        );
    }
}