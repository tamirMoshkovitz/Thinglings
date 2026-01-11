using System;
using _SLIME.BaseScripts;
using DG.Tweening;
using UnityEngine;
using UnityEngine.InputSystem;

namespace _SLIME.Tutorial
{

    [System.Serializable]
        public struct RockShakeSettings
        {
            public float shakeIntensity;
            public float shakeDuration;
            public float requiredShakeTime;
            public int shakeVibrato;
            public float shakeRandomness;
        }

       
    [RequireComponent(typeof(PlayerInput))]
    public class Rock: ProjectMonoBehavior
    {
        public static event Action JoystickMoved;
        [SerializeField] private TutorialScriptable tutorialScriptable;

        private RockShakeSettings rockShakeSettings;
        private bool _isShaking;
        private float _accumulatedShakeTime;
        private Tween _currentShakeTween;
        private bool _eventTriggered;
        
        private void OnEnable()
        {
            _isShaking = false;
            _accumulatedShakeTime = 0f;
        }
        
        private void OnDisable()
        {
            StopShake();
        }

        private void Awake()
        {
            rockShakeSettings = tutorialScriptable.RockShakeSettings;
        }

        private void Update()
        {
            if (_isShaking)
            {
                _accumulatedShakeTime += Time.deltaTime;
                if (_accumulatedShakeTime >= rockShakeSettings.requiredShakeTime)
                {
                    JoystickMoved?.Invoke();
                    _accumulatedShakeTime = 0f;
                }
            }
        }
        
        public void OnMove(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                StartShake();
            }
            else if (context.canceled)
            {
                StopShake();
            }
        }
        
        
        private void StartShake()
        {
            if (_isShaking) return;
            
            _isShaking = true;
            
            _currentShakeTween = transform.DOShakePosition(
                rockShakeSettings.shakeDuration,
                rockShakeSettings.shakeIntensity,
                rockShakeSettings.shakeVibrato,
                rockShakeSettings.shakeRandomness,
                false,
                true)
                .SetLoops(-1);
        
        }
        
        private void StopShake()
        {
            if (!_isShaking) return;
            
            _isShaking = false;
            _currentShakeTween?.Kill();
            _currentShakeTween = null;
            _accumulatedShakeTime = 0f;
           transform.DOKill();
           transform.localPosition = Vector3.zero;
        }
    }
}