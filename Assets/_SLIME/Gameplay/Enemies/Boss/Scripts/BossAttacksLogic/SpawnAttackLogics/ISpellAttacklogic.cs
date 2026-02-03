using System;
using NaughtyAttributes;
using UnityEngine;

namespace _SLIME.Boss
{
    [Serializable]
    public struct SpellSettings
    {
        [Tooltip("1f for perfect accuracy, 0f for awful accuracy")]
        [MinMaxSlider(0F, 1F)] public Vector2 attackAccuracyRange;
        [MinMaxSlider(0F, 100F)] public Vector2 attackSpeedRange;
        [Tooltip("Scale-up curve (0->0, 1->1). Duration = scaleUpDurationFactor / moveSpeed.")]
        public AnimationCurve scaleUpCurve;
        public float scaleUpDurationFactor;
        [Tooltip("Starting scale for scale-up animation.")]
        public Vector3 scaleStart;
        [Tooltip("When distance to slime < this, scale up very fast (duration = scaleUpDurationWhenClose).")]
        public float scaleUpCloseDistanceThreshold;
        public float scaleUpDurationWhenClose;
        [Tooltip("Lob arc: height above direct line (up then down to target). 0 = straight line.")]
        public float lobArcHeight;
    }
    
    [Serializable]
    public struct SpellSpecialAttacksSettings
    {
        [Range(0f, 360f)]
        public float threeShotsAngle;
        public float fourShotsWaitBetweenShots;
        
        [Range(0f, 360f)]
        public float bulletHellTotalAngle;
        public float bulletHellWaitBetweenShots;
        
        [Tooltip("Controls how dense Bullet Hell shots are in the middle (1 = linear, >1 = more dense in middle)")]
        [Range(0.5f, 3f)]
        public float bulletHellMiddleDensityPower;
    }
    public interface ISpellAttackLogic
    {
        // is the Attack logic active?
        bool IsActive { get; }
        // Start the attack logic
        void Attack(SpellSettings spellSets);
        // Update the attack logic (if needed)
        void UpdateAttack();
        // Reset the attack logic (if needed)
        void Reset();
    }
}