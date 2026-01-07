using System;
using _SLIME.BaseScripts;
using _SLIME.GameLoop;
using UnityEngine;

namespace _SLIME.Scenes.Pinball.Scripts
{
    [RequireComponent(typeof(Animator), typeof(Rigidbody2D))]
    public class Icicle: ProjectMonoBehavior
    {
        [SerializeField] private BoxCollider2D fullCollider;
        [SerializeField] private BoxCollider2D brokenCollider;
        [SerializeField] private float fallingGravityScale = 1f;
        
        private static readonly int BreakHash = Animator.StringToHash("Break");
        private static readonly int Crumble = Animator.StringToHash("Crumble");
        private Animator _animator;
        private Renderer _renderer;
        private Rigidbody2D _rigidbody;

        public void Break()
        {
            GameEvents.IcicleGotHit?.Invoke();
            _animator.SetTrigger(BreakHash);
            
            // physics
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
            if (collision.gameObject.CompareTag("Wall"))
            {
                _animator.SetTrigger(Crumble);
                _rigidbody.gravityScale = 0f;
                _rigidbody.linearVelocity = Vector2.zero;
            }
        }
    }
}