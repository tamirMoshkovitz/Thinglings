using System;
using Player.Interfaces;
using Unity.VisualScripting.Dependencies.Sqlite;
using UnityEngine;
using UnityEngine.InputSystem;

[Serializable]
public class ControllerRumble : ISlimeBehaviorComponent
{
    [Header("Controller Type Name")]
    [SerializeField] String controllerTypeName;
    
    [Header("Tear Rumble Settings")]
    [SerializeField] private bool addTearRumble = true;
    [SerializeField] public float tearRumbleDuration= .2f;
    [SerializeField][Range(0, 1)] private float tearRumbleLowFrequency = .2f;
    [SerializeField][Range(0, 1)] private float tearRumbleHighFrequency = 1f;
    
    [Header("Stretch Rumble Settings")]
    [SerializeField][Range(0, 1)] private float stretchRumbleLowFrequency = .5f;
    [SerializeField][Range(0, 1)] private float stretchRumbleHighFrequency = .5f;
    [SerializeField] private float rumbleChangeThreshold = 0.02f;
    
    private float _prevLow;

    private float _prevHigh;

    private SlimeData _slimeData;

    public ISlimeBehaviorComponent Awake(SlimeData slimeData)
    {
        _slimeData = slimeData;
        return this;
    }

    public void UpdateStretch()
    {
        float lowFrequency = CalculateStretchRumble(stretchRumbleLowFrequency);
        float highFrequency = CalculateStretchRumble(stretchRumbleHighFrequency);
        if (Mathf.Abs(lowFrequency - _prevLow) > rumbleChangeThreshold ||
            Mathf.Abs(highFrequency - _prevHigh) > rumbleChangeThreshold)
        {
            Gamepad.current?.SetMotorSpeeds(lowFrequency, highFrequency);
            _prevLow = lowFrequency;
            _prevHigh = highFrequency;
        }
    }

    public void OnPauseGame()
    {
        StopRumble();
    }

    public void OnSlimeTears()
    {
        StopRumble();
        _slimeData.ReachedMaxStretch = false;
        TearRumble(tearRumbleLowFrequency, tearRumbleHighFrequency);
    }
    
    public void OnDestroy()
    {
        Gamepad.current?.ResetHaptics();
    }

    public void OnResumeGame()
    {
        Gamepad.current?.ResumeHaptics();
    }

    private void TearRumble(float lowFrequency, float highFrequency)
    {
        if (_slimeData.ReachedMaxStretch) return;

        _slimeData.ReachedMaxStretch = true;
        StopRumble();
        if (addTearRumble)
        {
            Gamepad.current?.SetMotorSpeeds(lowFrequency, highFrequency);
        }
    }

    public void OnTearFinished()
    {
        StopRumble();
    }

    private void StopRumble()
    {
        Gamepad.current?.ResetHaptics();  
        Gamepad.current?.SetMotorSpeeds(0f, 0f);

        _slimeData.ReachedMaxStretch = true;
    }

    private float CalculateStretchRumble(float baseFrequency)
    {
        float normalized = Mathf.Clamp01(_slimeData.StretchRatio);
        return (Mathf.Pow(normalized, 2f)) * baseFrequency; ;
    }
}