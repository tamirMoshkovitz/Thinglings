using System;
using Audio;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Player.Brick
{
    public class BrickBehavior: MonoBehaviour
    {
        [SerializeField] private int damage = 20;
        [SerializeField] private float lifeTime = 3f;
        
        private static readonly int dissolveId = Shader.PropertyToID("_Dissolve");
        private MaterialPropertyBlock _propBlock;
        
        private Rigidbody2D _rb;
        private Renderer _renderer;
        private InputAction _shootAction;
        private bool _wasShot;
        private float _lifetimeTimer = 0f;
        
        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _renderer = GetComponent<Renderer>();
            _propBlock = new MaterialPropertyBlock();
        }

        private void Update()
        {
            if (_wasShot)
            {
                if (_lifetimeTimer >= lifeTime)
                {
                    Destroy(gameObject);
                }
                
                _lifetimeTimer += Time.deltaTime;
                
                float dissolveAmount = Mathf.Lerp(0f, dissolveId, Mathf.Clamp01(_lifetimeTimer / lifeTime));
                _renderer.GetPropertyBlock(_propBlock);
                _propBlock.SetFloat(dissolveId, dissolveAmount);
                _renderer.SetPropertyBlock(_propBlock);
            }
        }

        public void Shoot(float stretchForce)
        {
            _wasShot = true;
            damage = (int)(damage * stretchForce);
            GameEvents.BrickShot?.Invoke();
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (collision.collider.gameObject.TryGetComponent(typeof(DamageRelay), out Component boss))
            {
                ((DamageRelay)boss).TakeDamage(damage);
                Debug.Log($"<color=yellow>{gameObject.name}</color>");
            }
        }
    }
}