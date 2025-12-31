using _SLIME.BaseScripts;
using _SLIME.Boss;
using _SLIME.GameLoop;
using _SLIME.Gameplay.Projectiles.Brick.Scripts;
using UnityEngine;
using UnityEngine.InputSystem;

namespace _SLIME.Projectiles
{
    public class BrickBehavior: Spell
    {
        [SerializeField] private int damage = 20;
        
        private InputAction _shootAction;

        public override void Shoot(float stretchForce)
        {
            base.Shoot(stretchForce);
            DissolveCondition = true;
            damage = (int)(damage * stretchForce);
            GameEvents.BrickShot?.Invoke();
        }

        protected override void OnCollisionEnter2D(Collision2D other)
        {
            InitiateFastDissolve();
        }

        protected override void OnTriggerEnter2D(Collider2D collision)
        {
            var rig = collision.attachedRigidbody;
            if (rig && rig.TryGetComponent<IHealth>(out IHealth h))
            {
                h.TakeDamage(damage);
                // GameEvents.EnemyGotBricked?.Invoke(); //TODO: FIGURE OUT
            }
            InitiateFastDissolve();
        }
    }
}