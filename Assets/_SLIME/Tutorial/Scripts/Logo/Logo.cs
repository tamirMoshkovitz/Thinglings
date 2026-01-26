using System;
using _SLIME.BaseScripts;
using UnityEngine;
using UnityEngine.InputSystem;

namespace _SLIME.Tutorial
{
    [System.Serializable]
    public struct LogoSettings
    {
        public float requiredStretch;
        public float timeToGoDown;
    }
    
    public class Logo : ProjectMonoBehavior
    {
        private static readonly int StretchPower = Animator.StringToHash("StretchPower");
        private const int MaxNumber = 8;
        private const int MinNumber = 0;
        
        public static event Action JoystickMovedEnough;
        
        [SerializeField] private TutorialScriptable tutorialScriptable;
        [SerializeField] private Animator animator;
        
        private int _number = 0;
        private LogoSettings _settings;
        
        private bool _isRightHeld;
        private bool _isLeftHeld;
        private float _stretchTimer;
        private float _decayTimer;
        
        private void Awake()
        {
            _settings = tutorialScriptable.LogoSettings;
        }
        
        private void Update()
        {
            if (_isRightHeld && _isLeftHeld)
            {
               
                _stretchTimer += Time.deltaTime;
                _decayTimer = 0f;
                
                if (_stretchTimer >= _settings.requiredStretch)
                {
                    _stretchTimer = 0f;
                    IncreaseNumber();
                }
            }
            else
            {
                // Not both held - decay over time
                _stretchTimer = 0f;
                _decayTimer += Time.deltaTime;
                
                if (_decayTimer >= _settings.timeToGoDown)
                {
                    _decayTimer = 0f;
                    DecreaseNumber();
                }
            }
        }
        
        private void IncreaseNumber()
        {
            if (_number >= MaxNumber) return;
            
            _number++;
            UpdateAnimator();
            
            if (_number >= MaxNumber)
            {
                JoystickMovedEnough?.Invoke();
            }
        }
        
        private void DecreaseNumber()
        {
            if (_number <= MinNumber) return;
            
            _number--;
            UpdateAnimator();
        }
        
        private void UpdateAnimator()
        {
            animator.SetInteger(StretchPower, _number);
        }
        
        public void OnMoveRight(InputAction.CallbackContext context)
        {
            if (context.canceled)
            {
                _isRightHeld = false;
                return;
            }
            
            Vector2 input = context.ReadValue<Vector2>();
            _isRightHeld = input.x > 0; // Right joystick must point right (+X)
        }
        
        public void OnMoveLeft(InputAction.CallbackContext context)
        {
            if (context.canceled)
            {
                _isLeftHeld = false;
                return;
            }
            
            Vector2 input = context.ReadValue<Vector2>();
            _isLeftHeld = input.x < 0; // Left joystick must point left (-X)
        }
    }
}