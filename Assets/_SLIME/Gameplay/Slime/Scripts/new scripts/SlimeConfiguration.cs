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
            [FormerlySerializedAs("maxStretch")] public float breakForce;
            public float connectionFrequency;
            public float connectionDampingRatio;
        }

        
        
        
        [Tab("Slime")] 
        [SerializeField] private SlimeConfigurationFormat slimeConfiguration;
        [Tab("Slime")] 
        [SerializeField] public LineSettings LineDefaultSettings;
        
        [Tab("Slime")] 
        [SerializeField] public TrampolinePowerSettings TrampolinePowerSettings;
        public bool IsConnectedAtStart => slimeConfiguration.isConnectedAtStart;
        public float MoveSpeed => slimeConfiguration.moveSpeed;
        public int MaxHealth => slimeConfiguration.maxHealth;

        public float BreakForce => slimeConfiguration.breakForce;
        
        public float ConnectionFrequency => slimeConfiguration.connectionFrequency;
        public float ConnectionDampingRatio => slimeConfiguration.connectionDampingRatio;

        
        


    }
}