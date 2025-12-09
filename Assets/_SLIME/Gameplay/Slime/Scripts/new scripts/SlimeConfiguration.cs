using UnityEngine;
using UnityEngine.Serialization;

namespace _SLIME.Gameplay.Slime.Scripts.new_scripts
{

    [CreateAssetMenu(fileName = "SlimeConfig", menuName = "Scriptable Objects/SlimeConfiguration")]
    public class SlimeConfiguration : TabbedScriptableObject
    {
        [System.Serializable]
        public struct SlimeConfigurationFormat
        {
            public bool isConnectedAtStart;
            public float moveSpeed;
            public int maxHealth;
            public float maxStretch;
            public float connectionFrequency;
            public float connectionDampingRatio;
        }

        
        
        
        [Tab("Slime")] 
        [SerializeField] private SlimeConfigurationFormat slimeConfiguration;
        [Tab("Slime")] 
        [SerializeField] public LineSettings LineDefaultSettings;
        public bool IsConnectedAtStart => slimeConfiguration.isConnectedAtStart;
        public float MoveSpeed => slimeConfiguration.moveSpeed;
        public int MaxHealth => slimeConfiguration.maxHealth;

        public float MaxStretch => slimeConfiguration.maxStretch;
        
        public float ConnectionFrequency => slimeConfiguration.connectionFrequency;
        public float ConnectionDampingRatio => slimeConfiguration.connectionDampingRatio;

        
        


    }
}