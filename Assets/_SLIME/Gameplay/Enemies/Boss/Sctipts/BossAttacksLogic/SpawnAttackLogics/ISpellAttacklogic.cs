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