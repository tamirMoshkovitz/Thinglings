using NUnit.Framework;
using UnityEngine;

namespace _SLIME.Gameplay.Slime.Scripts.new_scripts
{
    public class SlimeSideHealth // TODO: think about the health logic, what is the main body health, how is the healing is done, etc.
    {
        public struct SlimeSideHealthFormat
        {
            public SlimeSide Parent;
            public float MaxHealth;
            public GameObject PlayerHitPoint;
            
            public SlimeSideHealthFormat(SlimeSide parent, float maxHealth, GameObject playerHitPoint)
            {
                Parent = parent;
                MaxHealth = maxHealth;
                PlayerHitPoint = playerHitPoint;
            }
        }
        
        #region Properties
        public float CurrentHealth { get; private set; }
        public bool IsDead { get; private set; }
        #endregion
        
        #region Private Fields
        private SlimeSide _parent;
        private GameObject _playerHitPoint;
        private readonly float _maxHealth;
        #endregion
        
        public SlimeSideHealth(SlimeSideHealthFormat format)
        {
            _parent = format.Parent;
            _maxHealth = format.MaxHealth;
            _playerHitPoint = format.PlayerHitPoint;
            CurrentHealth = _maxHealth;
        }

        public void OnEnable()
        {
            SlimeEvents.SlimeConnected += OnSlimeConnected;
            SlimeEvents.SlimeTears += OnSlimeTear;
            SlimeEvents.SlimeGetHit += OnSlimeGetHit;
        }

        public void OnDisable()
        {
            SlimeEvents.SlimeConnected -= OnSlimeConnected;
            SlimeEvents.SlimeTears -= OnSlimeTear;
            SlimeEvents.SlimeGetHit -= OnSlimeGetHit;
        }

        public void Update() { }

        public void TakeDamage(float damage)
        {
            if (IsDead) return;

            CurrentHealth -= damage;
            if (CurrentHealth <= 0)
            {
                CurrentHealth = 0;
                IsDead = true;
            }
        }

        public void Resurrect()
        {
            IsDead = false;
        }

        private void OnSlimeConnected()
        {
            Resurrect();
        }

        private void OnSlimeTear() { }

        private void OnSlimeGetHit(GameObject hitSide)
        {
            if (hitSide ==_playerHitPoint)
            {
                IsDead = true;
            }
        }
    }
}