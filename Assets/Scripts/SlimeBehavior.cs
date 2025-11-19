using System.Collections.Generic;
using Player;
using Player.Interfaces;
using UnityEngine;

public class SlimeBehavior : MonoBehaviour
{
    [SerializeField] internal Transform rightCenter;
    [SerializeField] internal Transform leftCenter;
    [SerializeField] internal float maxStretch = 20f;
    [SerializeField] internal float connectionDistance = 2f;
    [SerializeField] private ControllerRumble controllerRumble;
    [SerializeField] private SlimeStretchCameraShake slimeStretchCameraShake;

    private SlimeData _slimeData;
    private readonly List<ISlimeBehaviorComponent> _components = new List<ISlimeBehaviorComponent>();
    
    private void Awake()
    {
        _slimeData = new SlimeData(this);
        _components.Add(controllerRumble.Awake(_slimeData));
        _components.Add(new SlimeAudio().Awake(_slimeData));
        _components.Add(slimeStretchCameraShake.Awake(_slimeData));
    }

    private void OnEnable()
    {
        GameEvents.PauseGame += OnPauseGame;
        GameEvents.ResumeGame += OnResumeGame;
    }
    
    private void OnDisable()
    {
        GameEvents.PauseGame -= OnPauseGame;
        GameEvents.ResumeGame -= OnResumeGame;
    }

    private void Update()
    {
        if (_slimeData.ReachedMaxStretch && _slimeData.Connected) return;

        if (_slimeData.Connected)
        {
            // Stretch rumble based on distance between left and right centers
            if (_slimeData.StretchRatio >= 1)
            {
                OnSlimeTears();
                return;
            }
            UpdateStretch();
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

public class SlimeData
{
    private SlimeBehavior slime;
    private static bool reachedMaxStretch = true;
    
    public SlimeData(SlimeBehavior slime)
    {
        this.slime = slime;
    }
    
    public float MaxStretch => slime.maxStretch;
    public float ConnectionDistance => slime.connectionDistance;

    public bool ReachedMaxStretch 
    {
        get => reachedMaxStretch;
        set => reachedMaxStretch = value;
    }

    public bool Connected
    {
        get
        {
            if (!ReachedMaxStretch ||
                Vector3.Distance(slime.leftCenter.position, slime.rightCenter.position) < slime.connectionDistance)
            {
                ReachedMaxStretch = false;
                return true;
            }

            return false;
        }
    }

    public float Distance => Vector3.Distance(slime.leftCenter.position, slime.rightCenter.position);

    public float StretchRatio => Distance / slime.maxStretch;
}
