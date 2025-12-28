using System;
using _SLIME.BaseScripts;
using _SLIME.LittleBoss;
using UnityEngine;

namespace _SLIME.Boss
{
    [CreateAssetMenu(fileName = "BaseBossConfig", menuName = "BossConfig")]
    public class BaseBossSettings : TabbedScriptableObject
    {
        [Serializable]
        public struct HandsAttackSettings
        {
            [Header("Hands Attack")]

            [Tooltip("How long the hand takes to complete the path")]
            public float handAttackDuration;

            [Tooltip("Time between individual smashes")]
            public float handCooldown;
        
            [Tooltip("Total number of hands to use during the attack")]
            public int totalHandsToUse;
        
            [Tooltip("If true, both hands will attack simultaneously")]
            public bool useBothHands;
        }
    
        [Serializable]
        public struct SpawnAttackSettings
        {
            [Header("Spawn Spells Attack")]
        
            [Tooltip("Projectile prefab to spawn")]
            public GameObject projectilePrefab;
        
            [Tooltip("How many spells the sorcerer will create during the attack")]
            public int spellsToCast;
        
            [Tooltip("How long will it take for a spell to spawn")]
            public float spawnInterval;
        
            [Tooltip("Life time of the spell in seconds")]
            public float spellLifeTime;
        
        }
    
        [Serializable]
        public struct LaserAttackSettings
        {
            [Header("Laser Attack")]
        
            [Tooltip("Animation curve for laser rotation over time")]
            public AnimationCurve rotationCurve;
        
            [Tooltip("Duration for which the lasers remain active")]
            public float rotationDuration;
        
            [Tooltip("Total laser rotations loops (full 360 degrees) during the attack")]
            public int totalLoops;
        }
    
        [Serializable]
        public struct BossCoreSettings
        {
            [Header("Health")]
            public float maxHealth;
            
        }
    
        [Tab("Attacks Settings")]
        [SerializeField] private HandsAttackSettings handsAttackSettings;
        [Tab("Attacks Settings")]
        [SerializeField] private SpawnAttackSettings spawnAttackSettings;
        [Tab("Attacks Settings")]
        [SerializeField] private LaserAttackSettings laserAttackSettings;
        [Tab("Attacks Settings")]
        [SerializeField] private LittleBossAttackSettings littleBossAttackSettings;


    
        [Tab("Boss Core Settings")]
        [SerializeField] private BossCoreSettings bossCoreSettings;
    
    
        public HandsAttackSettings HandsAttack => handsAttackSettings;
        public SpawnAttackSettings SpawnAttack => spawnAttackSettings;
        public LaserAttackSettings LaserAttack => laserAttackSettings;
    
        public LittleBossAttackSettings LittleBossAttack => littleBossAttackSettings;
        public BossCoreSettings CoreSettings => bossCoreSettings;
    }
}
