using System;
using _SLIME.BaseScripts;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

namespace _SLIME.Projectiles
{
    public class WaterSpell: Spell
    {
        [SerializeField] private float power;
        [SerializeField] private LayerMask slimeProjectileLayer;
        [SerializeField] private Vector3 screenCenter;
        [SerializeField] private float initialVelocity = 50f;

        private SpellSlimeAttributes _slimeAttributes;
        
        protected override void OnEnable()
        {
            base.OnEnable();

            _slimeAttributes = new SpellSlimeAttributes {
                deflectionPower = power,
                direction = ((screenCenter + Vector3.up * Random.Range(-1.25f, 1.25f)) - transform.position ).normalized,
                layerMask = slimeProjectileLayer
            };
        }

        void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Water Boss Collider")) 
                HandleImpact(null);
        }

        void OnCollisionEnter2D(Collision2D other)
        {
            if (other.gameObject.CompareTag("Water Boss Collider")) 
                HandleImpact(null);
        }

        protected override void HandleImpact(IHealth target)
        {
            base.HandleImpact(null);
        }

        protected override void Shoot()
        {
            _currentState = SpellState.Flying;
            comp.rb.linearVelocity = _slimeAttributes.direction.normalized;
            comp.animator.Play("SlimeSpellMovement");
        }

        public override void OnSpawnFinished()
        {
            _currentState = SpellState.Flying;
            comp.rb.linearVelocity = _slimeAttributes.direction.normalized * initialVelocity;
            Deflect(_slimeAttributes);
        }
    }
}