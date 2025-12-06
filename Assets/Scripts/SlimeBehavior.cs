using System;
using System.Collections.Generic;
using Player;
using Player.Interfaces;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SlimeBehavior : MonoBehaviour
{
    [SerializeField] internal Transform rightCenter;
    [SerializeField] internal Transform leftCenter;
    [SerializeField] internal float maxStretch = 20f;
    [SerializeField] internal float connectionDistance = 2f;
    [SerializeField] private ControllerRumble controllerRumble;
    [SerializeField] private SlimeStretchCameraShake slimeStretchCameraShake;

    private SlimeData _slimeData;
    private PlayerMovement _rightCenterPlayerMovement, _leftCenterPlayerMovement;
    private readonly List<ISlimeBehaviorComponent> _components = new ();
    
    private void Awake()
    {
        _rightCenterPlayerMovement = rightCenter.GetComponent<PlayerMovement>();
        _leftCenterPlayerMovement = leftCenter.GetComponent<PlayerMovement>();
        
        _slimeData = SlimeData.Instance(this);
        _components.Add(controllerRumble.Awake(_slimeData));
        _components.Add(new SlimeAudio().Awake(_slimeData));
        _components.Add(slimeStretchCameraShake.Awake(_slimeData));
    }

    private void OnEnable()
    {
        GameEvents.PauseGame += OnPauseGame;
        GameEvents.ResumeGame += OnResumeGame;
        GameEvents.slimeConnected += OnSlimeConnected;
        GameEvents.SlimeTears += OnSlimeTears;
        GameEvents.BrickShot += OnBrickShot;
    }

    private void OnDisable()
    {
        GameEvents.PauseGame -= OnPauseGame;
        GameEvents.ResumeGame -= OnResumeGame;
        GameEvents.slimeConnected -= OnSlimeConnected;
        GameEvents.SlimeTears -= OnSlimeTears;
        GameEvents.BrickShot -= OnBrickShot;
    }

    private void Update()
    {
        if (_rightCenterPlayerMovement.MovementLocked && _leftCenterPlayerMovement.MovementLocked)
        {
            Debug.Log("You lost");
        }
        
        if (_slimeData.ReachedMaxStretch && _slimeData.Connected) return;

        if (_slimeData.Connected)
        {
            // Stretch rumble based on distance between left and right centers
            if (_slimeData.StretchRatio >= 1)
            {
                GameEvents.SlimeTears?.Invoke();
                return;
            }
            UpdateStretch();
        }
    }

    private void OnDestroy()
    {
        foreach (var component in _components)
        {
            component.OnDestroy();
        }
    }

    private void OnSlimeConnected()
    {
        foreach (var component in _components)
        {
            component.OnSlimeConnected();
        }
    }

    private void OnSlimeTears()
    {
        foreach (var component in _components)
        {
            component.OnSlimeTears();
        }
        Invoke(nameof(OnTearFinished), controllerRumble.tearRumbleDuration);
    }

    private void OnBrickShot()
    {
        Invoke(nameof(OnSlimeTears), .3f);
    }

    private void OnTearFinished()
    {
        foreach (var component in _components)
        {
            component.OnTearFinished();
        }
    }
    
    private void UpdateStretch()
    {
        foreach (var component in _components)
        {
            component.UpdateStretch();
        }
    }
    
    private void OnPauseGame()
    {
        foreach (var component in _components)
        {
            component.OnPauseGame();
        }
    }
    
    private void OnResumeGame()
    {
        foreach (var component in _components)
        {
            component.OnResumeGame();
        }
    }
}