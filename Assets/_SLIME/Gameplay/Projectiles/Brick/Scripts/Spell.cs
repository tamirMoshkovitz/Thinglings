using System;
using _SLIME.BaseScripts;
using _SLIME.GameLoop;
using UnityEngine;

namespace _SLIME.Gameplay.Projectiles.Brick.Scripts
{
    public abstract class Spell: ProjectMonoBehavior
    {
        [SerializeField] private float lifeTime = 1200f;
        [SerializeField] private float fastDissolveLifeTime = 400f;

        protected bool DissolveCondition = false;
        protected Rigidbody2D Rb;

        private Renderer _renderer;
        private static readonly int DissolveId = Shader.PropertyToID("_Dissolve");
        private MaterialPropertyBlock _propBlock;
        private float _lifetimeTimer = 0f;

        private void Awake()
        {
            Rb = GetComponent<Rigidbody2D>();
            _renderer = GetComponent<Renderer>();
            _propBlock = new MaterialPropertyBlock();
        }
        
        private void Update()
        {
            if (DissolveCondition)
            {
                if (_lifetimeTimer >= lifeTime)
                {
                    Destroy(gameObject);
                }
                
                _lifetimeTimer += Time.deltaTime;
                
                float dissolveAmount = Mathf.Lerp(0f, DissolveId, Mathf.Clamp01(_lifetimeTimer / lifeTime));
                _renderer.GetPropertyBlock(_propBlock);
                _propBlock.SetFloat(DissolveId, dissolveAmount);
                _renderer.SetPropertyBlock(_propBlock);
            }
        }
        
        public virtual void Shoot(float stretchForce)
        {
            GameEvents.BrickShot?.Invoke();
        }

        protected virtual void OnCollisionEnter2D(Collision2D other) // All derived classes should implement this
        {
            throw new NotImplementedException();
        }

        protected virtual void OnTriggerEnter2D(Collider2D other)
        {
            throw new NotImplementedException();
        }
        
        protected void InitiateFastDissolve()
        {
            lifeTime = fastDissolveLifeTime;
            DissolveCondition = true;
            
            // disable all children colliders to prevent further interactions
            Collider2D[] colliders = GetComponentsInChildren<Collider2D>();
            foreach (Collider2D childCollider in colliders)
            {
                childCollider.enabled = false;
            }
        }
    }
}