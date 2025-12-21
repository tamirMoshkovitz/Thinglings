using System;
using _SLIME.BaseScripts;
using UnityEngine;

namespace _SLIME.Slime
{
    [CreateAssetMenu(fileName = "ControllerRumbleConfig", menuName = "Scriptable Objects/Controller Rumble Configuration")]
    public class ConrollerRumbleConfiguration : TabbedScriptableObject
    {
        [Serializable]
        public struct ControllerRumbleConfigurationFormat
        {
            [Header("Tear Rumble Settings")]
            public bool addTearRumble;
            public float tearRumbleDuration;
            [Range(0, 1)] public float tearRumbleLowFrequency;
            [Range(0, 1)] public float tearRumbleHighFrequency;
            
            [Header("Stretch Rumble Settings")]
            [Range(0, 1)] public float stretchRumbleLowFrequency;
            [Range(0, 1)] public float stretchRumbleHighFrequency;
            public float rumbleChangeThreshold;
        }
        

        [Tab("Rumble Settings")]
        [SerializeField] private ControllerRumbleConfigurationFormat controllerRumbleConfiguration;
        public bool AddTearRumble => controllerRumbleConfiguration.addTearRumble;
        public float TearRumbleDuration => controllerRumbleConfiguration.tearRumbleDuration;
        public float TearRumbleLowFrequency => controllerRumbleConfiguration.tearRumbleLowFrequency;
        public float TearRumbleHighFrequency => controllerRumbleConfiguration.tearRumbleHighFrequency;
        
        public float StretchRumbleLowFrequency => controllerRumbleConfiguration.stretchRumbleLowFrequency;
        public float StretchRumbleHighFrequency => controllerRumbleConfiguration.stretchRumbleHighFrequency;
        public float RumbleChangeThreshold => controllerRumbleConfiguration.rumbleChangeThreshold;
    }
}