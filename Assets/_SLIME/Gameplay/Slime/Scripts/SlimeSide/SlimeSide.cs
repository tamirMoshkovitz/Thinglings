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
            public SlimeSideShootingReqComponents ShootingReqComponents;
            public SlimeSideShootingSettings ShootingSettings;
            public SlimeSideFormat(GameObject gameObject, Transform topTransform,
                float moveSpeed, int maxHealth, GameObject playerHitPoint, SlimeData slimeData,
                SlimeSideShootingSettings shootingSettings,SlimeSideShootingReqComponents shootingReqComponents)
            {
                GameObject = gameObject;
                TopTransform = topTransform;
                MoveSpeed = moveSpeed;
                MaxHealth = maxHealth;
                PlayerHitPoint = playerHitPoint;
                SlimeData = slimeData;
                ShootingReqComponents = shootingReqComponents;
                ShootingSettings = shootingSettings;
            }
        }
        
        private GameObject _gameObject;
        private SlimeSideMovement _movement;
        private SlimeSideHealth _health;
        private SlimeData _slimeData;
        private SlimeAnimatorController _animatorController;
        private readonly SlimeSideShooting _shooter;

        public SlimeSide(SlimeSideFormat format)
        {
            _slimeData = format.SlimeData;
            _gameObject = format.GameObject;
            _animatorController = new SlimeAnimatorController(_gameObject.GetComponent<Animator>(),
                format.ShootingReqComponents.renderer);
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
                _animatorController,
                _slimeData
            ));

            _shooter = new SlimeSideShooting(this,
                format.ShootingSettings,
                format.ShootingReqComponents);

        }
        
        public Vector3 Position => _gameObject.transform.position;
        public Transform Transform => _gameObject.transform;
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

        public void OnShoot(InputAction.CallbackContext context)
        {
            if (!_slimeData.Connected) _shooter.OnShoot(context);
        }

        public float Mass
        {
            get
            {
                float mass = 0;
                foreach (var rb in _gameObject.GetComponentsInChildren<Rigidbody2D>())
                {
                    if(rb.gameObject.CompareTag($"NotIncludeRBToMass")) continue;
                    mass += rb.mass;
                }

                return mass;
            }
        }
        
    }
}
