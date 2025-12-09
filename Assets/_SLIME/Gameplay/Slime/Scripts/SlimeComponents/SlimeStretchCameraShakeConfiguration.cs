using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace _SLIME.Gameplay.Slime.Scripts.new_scripts
{
    [CreateAssetMenu(fileName = "SlimeStretchCameraShakeConfig", menuName = "Scriptable Objects/Slime Stretch Camera Shake Configuration")]
    public class SlimeStretchCameraShakeConfiguration : TabbedScriptableObject
    {
        [Serializable]
        public struct StretchCameraShakeConfigurationFormat
        {
            [Header("Stretch Camera Shake Settings")]
            public float shakeDuration;
            public float tearShakeStrength;
            public float stretchShakeStrength;
            public float shakeUpdateFrequency;
        }
        
        [Tab("Stretch Camera Shake Settings")]
        [SerializeField] private StretchCameraShakeConfigurationFormat stretchCameraShakeConfiguration;
        public float ShakeDuration => stretchCameraShakeConfiguration.shakeDuration;
        public float TearShakeStrength => stretchCameraShakeConfiguration.tearShakeStrength;
        public float StretchShakeStrength => stretchCameraShakeConfiguration.stretchShakeStrength;
        public float ShakeUpdateFrequency => stretchCameraShakeConfiguration.shakeUpdateFrequency;
    }
}