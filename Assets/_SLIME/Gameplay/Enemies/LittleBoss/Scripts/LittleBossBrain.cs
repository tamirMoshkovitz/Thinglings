using System;
using _SLIME.BaseScripts;
using _SLIME.Boss;
using UnityEngine;
using UnityEngine.Serialization;

namespace _SLIME.LittleBoss
{
    
    
    [Serializable]
    public struct LittleBossStatesRef
    {
        public LittleBossMovementRef movRef;
        public LittleBossSpellAttackRef spellRef;
        // public LittleBossHealthRef healthRef;
    }
    public class LittleBossBrain: ProjectMonoBehavior
    {
        [Tooltip("If We want to examine a set without it to change through phase")]
        [SerializeField] private bool enableSetChange;
        
        
        [Header("General References")]
        [SerializeField] private Animator animator;
        
        [Header("State References")]
        [SerializeField] private LittleBossStatesRef reference;
        
        public LittleBossMovementRef MovRef => reference.movRef;
        public LittleBossSpellAttackRef SpellRef => reference.spellRef;
        
        // public LittleBossHealthRef HealthRef => reference.healthRef;
        void Start()
        {
            foreach (var state in animator.GetBehaviours<LittleBossBaseState>())
                state.Init(this);
        }

        private void UpdateSettings(BaseBossConfigurations newSettings)
        {
            if(!enableSetChange) return;
            foreach (var state in animator.GetBehaviours<LittleBossBaseState>())
                state.UpdateSet(newSettings);
        }
        
    }
}