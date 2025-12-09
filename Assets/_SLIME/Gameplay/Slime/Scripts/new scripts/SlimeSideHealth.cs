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
            
            public SlimeSideHealthFormat(SlimeSide parent, float maxHealth)
            {
                Parent = parent;
                MaxHealth = maxHealth;
            }
        }
        
        #region Properties
        public float CurrentHealth { get; private set; }
        public bool IsDead { get; private set; }
        #endregion
        
        #region Private Fields
        private SlimeSide _parent;
        private readonly float _maxHealth;
        #endregion
        
        public SlimeSideHealth(SlimeSideHealthFormat format)
        {
            _parent = format.Parent;
            _maxHealth = format.MaxHealth;
            CurrentHealth = _maxHealth;
        }

        public void OnEnable()
        {
            SlimeEvents.SlimeConnected += OnSlimeConnected;
            SlimeEvents.SlimeTears += OnSlimeTear;
        }

        public void OnDisable()
        {
            SlimeEvents.SlimeConnected -= OnSlimeConnected;
            SlimeEvents.SlimeTears -= OnSlimeTear;
        }

        public void Update() { }

        public void FixedUpdate()
        {
            if (!IsDead)
            {
                CheckIfSmashed();
            }
        }

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

        private void CheckIfSmashed()
        {
            // raycast up and down to check hand from above and floor from below
            RaycastHit2D hitUp = Physics2D.Raycast(_parent.Position, Vector2.up * 1.5f, LayerMask.NameToLayer("Enemy Projectiles"));
        
            if (hitUp.collider && hitUp.collider.CompareTag("Smasher")) // TODO: replace with relevant layer or tag
            {
                IsDead = true;
                // slimeEyes.SetActive(false); TODO: Notify parent SlimeSide that it is dead
            }
        }

        private void OnSlimeConnected()
        {
            Resurrect();
        }

        private void OnSlimeTear() { }
    }
}