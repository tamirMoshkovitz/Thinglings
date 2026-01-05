using _SLIME.BaseScripts;
using _SLIME.Projectiles;
using UnityEngine;
using UnityEngine.Serialization;

namespace _SLIME.Slime
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
            public float maxStretchTimeThreshold; 
            public float maxStretchPercentThreshold; 
        }

        
        
        
        [FormerlySerializedAs("slimeConfiguration")]
        [Tab("Slime")] 
        [SerializeField] private SlimeConfigurationFormat slimeConfigurationNormal;
        [Tab("Slime")] 
        [SerializeField] public LineSettings LineDefaultSettings;
        
        [Tab("Slime")] 
        [SerializeField] public TrampolinePowerSettings TrampolinePowerSettings;
        [Tab("Slime")] 
        [SerializeField] public SparkPowerSettings SparkPowerSettings;
        public bool IsConnectedAtStart => slimeConfigurationNormal.isConnectedAtStart;
        public float MoveSpeed => slimeConfigurationNormal.moveSpeed;
        public int MaxHealth => slimeConfigurationNormal.maxHealth;

        public float BreakForce => slimeConfigurationNormal.breakForce;
        
        public float ConnectionFrequency => slimeConfigurationNormal.connectionFrequency;
        public float ConnectionDampingRatio => slimeConfigurationNormal.connectionDampingRatio;
        
        public float MaxStretchTimeThreshold => slimeConfigurationNormal.maxStretchTimeThreshold;
        public float MaxStretchPercentThreshold => slimeConfigurationNormal.maxStretchPercentThreshold;

        [Tab("Slime")] 
        [SerializeField] public int MaxConnectionsOfSlime;
        
        [Tab("SlimeShootingPower")]
        [SerializeField] public SlimeSideShootingSettings shootingSettings;
        
    }
}