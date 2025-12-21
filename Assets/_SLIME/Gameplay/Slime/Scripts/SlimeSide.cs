using UnityEngine;
using UnityEngine.InputSystem;

namespace _SLIME.Slime
{
    public class SlimeSide
    {
        public struct SlimeSideFormat
        {
            public GameObject GameObject;
            public Transform TopTransform;
            public float MoveSpeed;
            public int MaxHealth;
            public GameObject PlayerHitPoint;
            public SlimeData SlimeData;

            public SlimeSideFormat(GameObject gameObject, Transform topTransform, float moveSpeed, int maxHealth, GameObject playerHitPoint, SlimeData slimeData)
            {
                GameObject = gameObject;
                TopTransform = topTransform;
                MoveSpeed = moveSpeed;
                MaxHealth = maxHealth;
                PlayerHitPoint = playerHitPoint;
                SlimeData = slimeData;
            }
        }
        
        private GameObject _gameObject;
        private SlimeSideMovement _movement;
        private SlimeSideHealth _health;
        private SlimeData _slimeData;
        private SlimeAnimatorController _animatorController;
        
        public SlimeSide(SlimeSideFormat format)
        {
            _gameObject = format.GameObject;
            _animatorController = new SlimeAnimatorController(_gameObject.GetComponent<Animator>());
            TopPosition = format.TopTransform;
            _movement = new SlimeSideMovement(new SlimeSideMovement.SlimeSideMovementFormat(
                this,
                format.GameObject.GetComponent<Rigidbody2D>(),
                format.MoveSpeed
            ));
            
            _health = new SlimeSideHealth(new SlimeSideHealth.SlimeSideHealthFormat(
                this,
                format.MaxHealth,
                format.PlayerHitPoint,
                new SlimeAnimatorController(_gameObject.GetComponent<Animator>())
            ));

            _slimeData = format.SlimeData;
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
            _animatorController.Update(_movement.IsMoving, _slimeData.IsStrained);
        }
        
        public void FixedUpdate()
        {
            _movement.FixedUpdate();
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
