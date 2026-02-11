using System;
using System.Collections;
using _SLIME.BaseScripts;
using _SLIME.Core.ControllerRumble.Scripts;
using _SLIME.Slime;
using DG.Tweening;
using FMODUnity;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

namespace _SLIME.Tutorial
{

    [System.Serializable]
        public struct RockShakeSettings
        {
            public float[] shakeIntensity;
            public float[] shakeDuration;
            public float[] requiredShakeTime;
            public int[] shakeVibrato;
            public float[] shakeRandomness;
        }

       
    [RequireComponent(typeof(PlayerInput))]
    public class Rock: ProjectMonoBehavior
    {
        public static event Action JoystickMoved;
        [SerializeField] private TutorialScriptable tutorialScriptable;
        [SerializeField] private ControlledSfx rockShakeSFX;
        [SerializeField] private ConrollerRumbleConfiguration rumbleConfiguration;
        

        private RockShakeSettings rockShakeSettings;
        private bool _isShaking;
        private float _accumulatedShakeTime;
        private Tween _currentShakeTween;
        private bool _eventTriggered;
        private int _currentShakeIndex = 0;
        private bool _isActive = true;

        private void OnEnable()
        {
            _isShaking = false;
            _accumulatedShakeTime = 0f;
            JoystickMoved += OnJoystickMoved;
        }
        
        private void OnDisable()
        {
            StopShake();
            JoystickMoved -= OnJoystickMoved;
            SetAudioIntensity(1f);
            DisableControllerShake();
        }

        public void OnRockTransform()
        {
            _isActive = false;
            StopShake();
        }
        
        
        public void OnRockFinishTransform()
        {
            _isActive = true;
        }
        private void Awake()
        {
            rockShakeSettings = tutorialScriptable.RockShakeSettings;
        }

        private void OnDestroy()
        {
            DisableControllerShake();
        }

        private void Update()
        {
            if (_isShaking)
            {
                _accumulatedShakeTime += Time.deltaTime;
                if (_accumulatedShakeTime >= rockShakeSettings.requiredShakeTime[_currentShakeIndex])
                {
                    JoystickMoved?.Invoke();
                    _accumulatedShakeTime = 0f;
                }
                UpdateAudioIntensity();
            }
        }
        
        private void OnJoystickMoved()
        {
            int maxIndex = Mathf.Min(
                rockShakeSettings.shakeIntensity.Length,
                rockShakeSettings.shakeDuration.Length,
                rockShakeSettings.requiredShakeTime.Length,
                rockShakeSettings.shakeVibrato.Length,
                rockShakeSettings.shakeRandomness.Length
            ) - 1;
            
            if (_currentShakeIndex < maxIndex)
            {
                _currentShakeIndex++;
            }
        }
        
        public void OnMove(InputAction.CallbackContext context)
        {
            if (!_isActive) return;
            
            if (context.performed)
            {
                if (!_isShaking)
                {
                    StartCoroutine(ShakeCoroutine());
                }
            }
            else if (context.canceled)
            {
                StopShake();
            }
        }
        
        private IEnumerator ShakeCoroutine()
        {
            _isShaking = true;
            rockShakeSFX.Play();
            
            GamepadWrapper.SetMotorSpeeds(rumbleConfiguration.StretchRumbleLowFrequency / 2f, rumbleConfiguration.StretchRumbleHighFrequency / 2f);
            while (_isShaking)
            {
                _currentShakeTween = transform.DOShakePosition(
                    rockShakeSettings.shakeDuration[_currentShakeIndex],
                    rockShakeSettings.shakeIntensity[_currentShakeIndex],
                    rockShakeSettings.shakeVibrato[_currentShakeIndex],
                    rockShakeSettings.shakeRandomness[_currentShakeIndex],
                    false,
                    true);
                
                yield return _currentShakeTween.WaitForCompletion();
            }
        }
        
        private void StopShake()
        {
            if (!_isShaking) return;
            
            _isShaking = false;
            rockShakeSFX.Stop();
            _currentShakeTween?.Kill();
            _currentShakeTween = null;
            _accumulatedShakeTime = 0f;
           transform.DOKill();
           transform.localPosition = Vector3.zero;

           DisableControllerShake();
        }

        private void DisableControllerShake()
        {
            GamepadWrapper.SetMotorSpeeds(0f,0f);
        }
        
        private void UpdateAudioIntensity()
        {
            SetAudioIntensity(CalculateIntensity());
        }

        private float CalculateIntensity()
        {
            float currentShake = _accumulatedShakeTime / rockShakeSettings.requiredShakeTime[_currentShakeIndex];
            
            return (_currentShakeIndex + currentShake) / rockShakeSettings.requiredShakeTime.Length;
        }
        
        private void SetAudioIntensity(float intensity)
        {
            RuntimeManager.StudioSystem.setParameterByName("first scene tension", intensity);
        }
    }
}