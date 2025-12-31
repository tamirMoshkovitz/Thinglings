using _SLIME.BaseScripts;
using _SLIME.Boss;
using _SLIME.GameLoop;
using _SLIME.Generics;
using _SLIME.Utils;
using UnityEngine;

namespace _SLIME.Projectiles
{ 
    public struct BulletInitData
    {
        public float speed;
        public float buffer;
        public float turnSmoothness; 
        public float damage;
        public Transform target;
        public Vector2 startPosition;
        public BulletMonoPool pool;

        /// <summary>
        /// Initializes a new BulletInitData instance with the specified parameters.
        /// </summary>
        /// <param name="target">Target transform to aim at.</param>
        /// <param name="startPosition">Starting position of the bullet.</param>
        /// <param name="speed">Movement speed of the bullet.</param>
        /// <param name="buffer">Distance buffer from start position.</param>
        /// <param name="pool">Pool to return the bullet to when deactivated.</param>
        /// <param name="turnSmoothness">Smoothing factor for bullet turning/tracking (0-1 range).</param>
        /// <param name="damage">Damage amount the bullet deals on impact.</param>
        public BulletInitData(Transform target, Vector2 startPosition,
                          float speed, float buffer, BulletMonoPool pool, float turnSmoothness, float damage)
        {
            this.target = target;
            this.startPosition = startPosition;
            this.speed = speed;
            this.buffer = buffer;
            this.pool = pool;
            this.turnSmoothness = turnSmoothness;
            this.damage = damage;
        }
    }
    public class Bullet : ProjectMonoBehavior, IPoolable
    {
        [Header("Bullet Components")]
        [Tooltip("Rigidbody2D component for physics-based movement.")]
        [SerializeField] private Rigidbody2D rb2D;

        [Header("Bullet State")]
        private BulletInitData _data;
        private bool _active;
        private Vector2 _currentDirection;
        
        [Tooltip("Logger component for weapon-specific debug messages.")]
        [SerializeField] protected GameLogger weaponLogger;
        
        
        [Header("Resume Game")] // IN FUTURE
        [Tooltip("Saved velocity for pause/resume functionality.")]
        private Vector2 _savedVelocity;


        

        /// <summary>
        /// Activates the bullet with the specified initialization data.
        /// Sets up position, rotation, and movement parameters.
        /// </summary>
        /// <param name="data">Initialization data containing all bullet parameters.</param>
        public void Activate(BulletInitData data)
        {
            if (data.target == null)
            {
                weaponLogger?.LogWarning("Bullet activation failed: target is null.");
                return;
            }

            weaponLogger?.Log("Bullet activated");
            _active = true;
            _data = data;

            Vector2 direction = ((Vector2)data.target.position - data.startPosition).normalized;
            _currentDirection = direction;
            Vector2 spawnPosition = data.startPosition + direction * data.buffer;

            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angle);

            transform.position = spawnPosition;
            rb2D.linearVelocity = direction * data.speed;
        }
        
        private void FixedUpdate()
        {   
            if (_data.target == null) return;
            
            Vector2 toTarget = (Vector2)_data.target.position - (Vector2)transform.position;
            Vector2 desiredDirection = toTarget.normalized;

            
            _currentDirection = Vector3.Slerp(_currentDirection, desiredDirection, _data.turnSmoothness).normalized;
            
            float angle = Mathf.Atan2(_currentDirection.y, _currentDirection.x) * Mathf.Rad2Deg;
            rb2D.MoveRotation(angle);
            rb2D.linearVelocity = _currentDirection * _data.speed;
        }

        /// <summary>
        /// Handles collision detection when the bullet hits a target.
        /// Deactivates the bullet and returns it to the pool.
        /// </summary>
        /// <param name="other">The collider that was hit.</param>
        public void OnTriggerEnter2D(Collider2D other)
        {
            if (!_active) return;
            weaponLogger?.Log($"Bullet triggered by: {other.name}");
            
            var rig = other.attachedRigidbody;
            if (rig && rig.TryGetComponent<IHealth>(out IHealth h))
            {
                h.TakeDamage((float)System.Math.Round(_data.damage, 2));
            }
        
            // Move bullet off-screen and return to pool
            transform.position = new Vector2(-100, -100);
            weaponLogger?.Log("Bullet returned to pool"); 
            _data.pool.Return(this);
        }

        /// <summary>
        /// Resets the bullet state for reuse from the pool.
        /// Clears all references and resets flags.
        /// </summary>
        public void Reset()
        {
            _active = false;
        }
        
        
        
        // TODO: IN FUTURE, GAME SHOULD SUPPORT PAUSE AND RESUME. THIS METHODS ARE FOR THIS.
        // /// <summary>
        // /// Called when the game is paused. Saves current velocity and stops movement.
        // /// </summary>
        // private void OnGamePaused()
        // {
        //     if (rb2D != null && rb2D.linearVelocity != Vector2.zero)
        //     {
        //         _savedVelocity = rb2D.linearVelocity;
        //         rb2D.linearVelocity = Vector2.zero;
        //     }
        // }

        // /// <summary>
        // /// Called when the game is resumed. Restores saved velocity and resumes movement.
        // /// </summary>
        // private void OnGameResumed()
        // {
        //     if (rb2D != null)
        //     {
        //         rb2D.linearVelocity = _savedVelocity;
        //     }
        // }
    }
}