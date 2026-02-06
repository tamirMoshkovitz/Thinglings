using System;
using _SLIME.BaseScripts;
using _SLIME.Boss;
using _SLIME.Slime;
using UnityEngine;
using UnityEngine.Serialization;

namespace _SLIME.LittleBoss
{
    [Serializable]
    public struct LittleBossStatesRef
    {
        public LittleBossMovementRef movRef;
        public LittleBossIdleRef idleRef;
        public LittleBossSpellAttackRef spellRef;
    }

    public class LittleBossBrain : ProjectMonoBehavior
    {
        [Tooltip("If We want to examine a set without it to change through phase")]
        [SerializeField] private bool enableSetChange;

        [Header("General References")]
        [SerializeField] private Animator animator;
        [SerializeField] private Transform hitPoint;
        [SerializeField] private Collider2D collider;
        [SerializeField] public GameObject Root;
        [Header("State References")]
        [SerializeField] private LittleBossStatesRef reference;

        public LittleBossMovementRef MovRef => reference.movRef;
        public LittleBossIdleRef IdleRef => reference.idleRef;
        public LittleBossSpellAttackRef SpellRef => reference.spellRef;

        private Vector3 _initialPosition;

        void Awake()
        {
            _initialPosition = transform.position;
            
        }

        void OnEnable()
        {
            foreach (var state in animator.GetBehaviours<LittleBossBaseState>())
                state.Init(this);
        }
        
        private void OnDisable()
        {
            transform.position = _initialPosition;
            transform.localScale = Vector3.zero;
            EnableCollider(false);
        }

        public void EnableCollider(bool enable) => collider.enabled = enable;
        
        public void DisableLittleBoss() => Root.SetActive(false);
        
        
    }
}