using UnityEngine;
using UnityEngine.InputSystem;

namespace _SLIME.Gameplay.Slime.Scripts.new_scripts
{
    public class SlimeSide
    {
        public struct SlimeSideFormat
        {
            public GameObject GameObject;
            public Transform TopTransform;
            public float MoveSpeed;
            public int MaxHealth;

            public SlimeSideFormat(GameObject game, Transform topTransform, float moveSpeed, int maxHealth)
            {
                GameObject = game;
                TopTransform = topTransform;
                MoveSpeed = moveSpeed;
                MaxHealth = maxHealth;
            }
        }
        
        private GameObject _gameObject;
        private SlimeSideMovement _movement;
        private SlimeSideHealth _health;
        
        public SlimeSide(SlimeSideFormat format)
        {
            _gameObject = format.GameObject;
            TopPosition = format.TopTransform;
            _movement = new SlimeSideMovement(new SlimeSideMovement.SlimeSideMovementFormat(
                this,
                format.GameObject.GetComponent<Rigidbody2D>(),
                format.MoveSpeed
            ));
            
            _health = new SlimeSideHealth(new SlimeSideHealth.SlimeSideHealthFormat(
                this,
                format.MaxHealth
            ));
        }
        
        public Vector3 Position => _gameObject.transform.position;
        public Transform TopPosition { get; private set; }
        public bool IsDead => _health.IsDead;
        
        public void OnEnable()
        {
            _movement.OnEnable();
            _health.OnEnable();
        }
        
        public void OnDisable()
        {
            _movement.OnDisable();
            _health.OnDisable();
        }
        
        public void Update()
        {
            _movement.Update();
            _health.Update();
        }
        
        public void FixedUpdate()
        {
            _movement.FixedUpdate();
            _health.FixedUpdate();
        }
        
        public void OnMove(InputAction.CallbackContext context)
        {
            _movement.OnMove(context);
        }

        public float Mass
        {
            get
            {
                float mass = 0;
                foreach (var rb in _gameObject.GetComponentsInChildren<Rigidbody2D>())
                {
                    mass += rb.mass;
                }

                return mass;
            }
        }
        
    }
}
