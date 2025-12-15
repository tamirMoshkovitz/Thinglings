using _SLIME.Gameplay.Slime.Scripts.new_scripts;
using Player.Interfaces;
using UnityEngine;
using UnityEngine.InputSystem;

namespace _SLIME.Gameplay.Slime.Scripts.SlimeComponents
{
    public class ControllerRumble : ISlimeBehaviorComponent
    {
        private readonly ConrollerRumbleConfiguration _configuration;
        private SlimeData _slimeData;
        private float _prevLow;
        private float _prevHigh;

        public ControllerRumble(ConrollerRumbleConfiguration configuration, SlimeData slimeData)
        {
            _configuration = configuration;
            _slimeData = slimeData;
        }

        public void UpdateStretch()
        {
            float lowFrequency = CalculateStretchRumble(_configuration.StretchRumbleLowFrequency);
            float highFrequency = CalculateStretchRumble(_configuration.StretchRumbleHighFrequency);
            if (Mathf.Abs(lowFrequency - _prevLow) > _configuration.RumbleChangeThreshold ||
                Mathf.Abs(highFrequency - _prevHigh) > _configuration.RumbleChangeThreshold)
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
            TearRumble(_configuration.TearRumbleLowFrequency, _configuration.TearRumbleHighFrequency);
        }

        public void OnSlimeConnected() { }

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
            StopRumble();
            if (_configuration.AddTearRumble)
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
        }

        private float CalculateStretchRumble(float baseFrequency)
        {
            float normalized = Mathf.Clamp01(_slimeData.StretchRatio);
            return (Mathf.Pow(normalized, 2f)) * baseFrequency; ;
        }
    }
}