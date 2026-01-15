using System;
using System.Collections.Generic;
using _SLIME.BaseScripts;
using UnityEngine;
[CreateAssetMenu(fileName = "BaseBossConfig", menuName = "BossConfig")]
public class BaseBossConfigurations : TabbedScriptableObject
{
    [Serializable]
    public struct HandsAttackSettings
    {
        [Header("Hands Attack")]
        [Tooltip("Hand warning duration before the attack starts")]
        public float handWarningDuration;

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
        
        [Tooltip("Spell Speed")]
        public float spellSpeed;
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
    
    [Serializable]
    public struct BossPhaseSettings
    {
        [Header("Phase Settings")]
        
        [Tooltip("Upper health threshold to enter this phase")]
        public float upperHealthThreshold;
        
        [Tooltip("Lower health threshold to exit this phase")]
        public float lowerHealthThreshold;
    }
    
    [Serializable]
    public struct IcicleSpawnSettings
    {
        [Header("Icicle Spawn Settings")]
        
        [Tooltip("Minimum time to wait before the next spawn.")]
        public float minWaitTime;
        
        [Tooltip("Maximum time to wait before the next spawn.")]
        public float maxWaitTime;
        
        [Tooltip("If true, it will keep spawning indefinitely.")]
        public bool loopSpawning;
    }
    
    [Tab("Attacks Settings")]
    [SerializeField] private HandsAttackSettings handsAttackSettings;
    [Tab("Attacks Settings")]
    [SerializeField] private SpawnAttackSettings spawnAttackSettings;
    [Tab("Attacks Settings")]
    [SerializeField] private LaserAttackSettings laserAttackSettings;
    [Tab("Attacks Settings")]
    [SerializeField] private IcicleSpawnSettings icicleSpawnSettings;
    
    
    [Tab("Boss Core Settings")]
    [SerializeField] private BossCoreSettings bossCoreSettings;
    
    [Tab("Boss Phase Settings")]
    [SerializeField] private BossPhaseSettings bossPhaseSettings;
    
    public HandsAttackSettings HandsAttack => handsAttackSettings;
    public SpawnAttackSettings SpawnAttack => spawnAttackSettings;
    public LaserAttackSettings LaserAttack => laserAttackSettings;
    public BossCoreSettings CoreSettings => bossCoreSettings;
    public BossPhaseSettings PhaseSettings => bossPhaseSettings;
    public IcicleSpawnSettings IcicleSpawn => icicleSpawnSettings;
}
