using System;
using _SLIME.BaseScripts;
using _SLIME.GameLoop;
using UnityEngine;

namespace _SLIME.Scenes.Pinball.Scripts
{
    [RequireComponent(typeof(Animator), typeof(Rigidbody2D))]
    public class Icicle: ProjectMonoBehavior, IHealth
    {
        [SerializeField] private BoxCollider2D fullCollider;
        [SerializeField] private BoxCollider2D brokenCollider;
        [SerializeField] private float fallingGravityScale = 1f;
        
        private static readonly int HitCount = Animator.StringToHash("Hit Count");
        private static readonly int Crumble = Animator.StringToHash("Crumble");
        private Animator _animator;
        private Renderer _renderer;
        private Rigidbody2D _rigidbody;
        private int _hitCounter = 0;
        
        private int HitCounter
        {
            get => _hitCounter;
            set
            {
                _hitCounter = value;
                _animator.SetInteger(HitCount, _hitCounter);
            }
        }
        
        
        public void Break() // add animation event call
        {
            _rigidbody.constraints -= RigidbodyConstraints2D.FreezePositionY;
            fullCollider.enabled = false;
            brokenCollider.enabled = true;
            _rigidbody.gravityScale = fallingGravityScale;
        }

        public void disableRenderer()
        {
            _renderer.enabled = false;
        }

        private void Awake()
        {
            _animator = GetComponent<Animator>();
            _rigidbody = GetComponent<Rigidbody2D>();
            _renderer = GetComponent<Renderer>();
        }

        private void Start()
        {
            fullCollider.enabled = true;
            brokenCollider.enabled = false;
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (collision.gameObject.CompareTag("Floor"))
            {
                _animator.SetTrigger(Crumble);
                _rigidbody.gravityScale = 0f;
                _rigidbody.linearVelocity = Vector2.zero;
                
                Invoke(nameof(SelfDestroy), 0.5f); // TODO: remove and use animation event
            }
        }

        private void SelfDestroy()
        {
            OnIcicleCrumbled();
            Destroy(gameObject);
        }

        public void TakeDamage(float damage = 0)
        {
            HitCounter++;
            GameEvents.IcicleGotHit?.Invoke();
        }
        
        private void OnIcicleCrumbled() // add animation event call
        {
            GameEvents.IcicleCrumbled?.Invoke();
        }
    }
}