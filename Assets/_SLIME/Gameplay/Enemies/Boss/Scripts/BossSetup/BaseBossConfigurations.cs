using System;
using System.Collections.Generic;
using _SLIME.BaseScripts;
using _SLIME.Boss;
using _SLIME.LittleBoss;
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
        
        public GameObject projectileBeforeSpawnPrefab;
        
        [Tooltip("How many spells the sorcerer will create during the attack")]
        public int attacksToCast;
        
        
        public SpawnSettings spawnSettings;
        
        [Tooltip("Spell Settings")]
        public SpellSettings spellSettings;
        
        [Tooltip("Special Attacks Settings")]
        public SpellSpecialAttacksSettings specialAttacksSettings;
    }
    
    [Serializable]
    public struct WaterAttackSettings
    {
        public float waterDamage;
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
        
        [Tooltip("Lower health threshold to exit this phase")]
        public float lowerHealthThreshold;
        
        [Tooltip("Number of spell hits to pass the phase")]
        public int targetHitsToKill;
        
        [Tooltip("Expected avg speed of spells in current phase")]
        public float expectedAvgSpeedOfSpells;
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

    [Tab("Attacks Settings")] [SerializeField]
    private WaterAttackSettings waterAttackSettings;
    
    [Tab("Boss Core Settings")]
    [SerializeField] private BossCoreSettings bossCoreSettings;
    
    [Tab("Boss Phase Settings")]
    [SerializeField] private BossPhaseSettings bossPhaseSettings;
    
    
    
    [Tab("Little Boss Settings")]
    [SerializeField] private LittleBossMovementSettings littleBossMovementSettings;
    [Tab("Little Boss Settings")]
    [SerializeField] private LittleBossSpellAttackSettings littleBossSpellAttackSettings;
    [Tab("Little Boss Settings")]
    [SerializeField] private LittleBossHealthSet littleBossHealthSettings;

    
    
    public HandsAttackSettings HandsAttack => handsAttackSettings;
    public SpawnAttackSettings SpawnAttack => spawnAttackSettings;
    public LaserAttackSettings LaserAttack => laserAttackSettings;
    public BossCoreSettings CoreSettings => bossCoreSettings;
    public BossPhaseSettings PhaseSettings => bossPhaseSettings;
    public IcicleSpawnSettings IcicleSpawn => icicleSpawnSettings;
    public LittleBossHealthSet LittleBossHealth => littleBossHealthSettings;
    public LittleBossMovementSettings LittleBossMovement => littleBossMovementSettings;
    public LittleBossSpellAttackSettings LittleBossSpellAttack => littleBossSpellAttackSettings;
    public WaterAttackSettings WaterAttack => waterAttackSettings;
}
