using System;
using System.Collections.Generic;
using _SLIME.Gameplay.Slime.Scripts.new_scripts;
using _SLIME.Gameplay.Slime.Scripts.SlimeComponents;
using Player.Interfaces;
using UnityEngine;

namespace Player.new_scripts
{
    public class SlimeFeelManager
    {
        private readonly ConrollerRumbleConfiguration _controllerRumbleConfiguration;
        private readonly float _onShotTearConnectionAfter;

        private SlimeBrain _slimeBrain;
        private List<ISlimeBehaviorComponent> _components = new ();

        public SlimeFeelManager(SlimeBrain slimeBrain, ConrollerRumbleConfiguration controllerRumbleConfiguration,
            SlimeStretchCameraShakeConfiguration slimeStretchCameraShakeConfiguration, Camera mainCamera, float onShotTearConnectionAfter)
        {
            _slimeBrain = slimeBrain;
            _controllerRumbleConfiguration = controllerRumbleConfiguration;
            _onShotTearConnectionAfter = onShotTearConnectionAfter;

            _components.Add(new ControllerRumble(controllerRumbleConfiguration, slimeBrain.Data));
            _components.Add(new SlimeAudio(slimeBrain.Data));
            _components.Add(new SlimeStretchCameraShake(slimeStretchCameraShakeConfiguration, slimeBrain.Data, mainCamera));
        }
        
        public void OnEnable()
        {
            GameEvents.PauseGame += OnPauseGame;
            GameEvents.ResumeGame += OnResumeGame;
            
            SlimeEvents.SlimeConnected += OnSlimeConnected;
            SlimeEvents.SlimeTears += OnSlimeTears;
        }
        
        public void OnDisable()
        {
            GameEvents.PauseGame -= OnPauseGame;
            GameEvents.ResumeGame -= OnResumeGame;
            
            SlimeEvents.SlimeConnected -= OnSlimeConnected;
            SlimeEvents.SlimeTears -= OnSlimeTears;
        }

        public void Update()
        {
            if (_slimeBrain.Data.ReachedMaxStretch && _slimeBrain.Data.Connected) return;

            if (_slimeBrain.Data.Connected)
            {
                // Stretch rumble based on distance between left and right centers
                if (_slimeBrain.Data.StretchRatio >= 1)
                {
                    GameEvents.SlimeTears?.Invoke();
                    return;
                }
                UpdateStretch();
            }
        }
        
        public void OnDestroy()
        {
            foreach (var component in _components)
            {
                component?.OnDestroy();
            }
        }
        
        private void OnSlimeConnected()
        {
            foreach (var component in _components)
            {
                component.OnSlimeConnected();
            }
        }
        
        public void OnSlimeTears()
        {
            foreach (var component in _components)
            {
                component.OnSlimeTears();
            }
            _slimeBrain.Invoke(nameof(OnTearFinished), _controllerRumbleConfiguration.TearRumbleDuration);
        }

        public void OnTearFinished()
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
}