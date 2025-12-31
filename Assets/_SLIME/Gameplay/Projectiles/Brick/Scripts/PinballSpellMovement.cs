using System;
using _SLIME.Scenes.Pinball.Scripts;
using UnityEngine;

namespace _SLIME.Gameplay.Projectiles.Brick.Scripts
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class PinballSpell: Spell
    {
        [SerializeField] private float maxSpeed = 25f;
        [SerializeField] private float bounceRandomness = 1f;
        [SerializeField] private int collisionToDissolve = 3;

        private float _shotTime;
        private bool _wasShot = false;
        private float _HitCount = 0;

        public override void Shoot(float stretchForce)
        {
            _wasShot = true;
            _shotTime = Time.time;
        }

        protected override void OnTriggerEnter2D(Collider2D other) { }

        protected override void OnCollisionEnter2D(Collision2D collision)
        {
            _HitCount++;
            
            if (collision.collider.gameObject.TryGetComponent(typeof(Icicle), out Component icicle))
            {
                ((Icicle)icicle).Break();
                DissolveCondition = true;
                Rb.linearVelocity = Vector2.zero;
            }
            
            
            float randomX = (UnityEngine.Random.value * 2 - 1f) * bounceRandomness;
            float randomY = (UnityEngine.Random.value * 2 - 1f) * bounceRandomness;
            Vector2 newDir = (Rb.linearVelocity + new Vector2(randomX, randomY)).normalized;
            Rb.linearVelocity = newDir * Rb.linearVelocity.magnitude;
        }

        private void FixedUpdate()
        {
            if (_HitCount >= collisionToDissolve)
            {
                InitiateFastDissolve();
            }
            
            if (Rb.linearVelocity.magnitude > maxSpeed)
            {
                Rb.linearVelocity = Rb.linearVelocity.normalized * maxSpeed;
            }
        }
    }
}