using System.Collections;
using FMODUnity;
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
            public EventReference DeadSlimeSFX;

            public Renderer SoulSprite;
            public SlimeSideFormat(GameObject gameObject, Transform topTransform,
                float moveSpeed, int maxHealth, GameObject playerHitPoint, SlimeData slimeData,
                SlimeSideShootingSettings shootingSettings,SlimeSideShootingReqComponents shootingReqComponents,
                EventReference deadSlimeSFX,Renderer soulSprite)
            {
                GameObject = gameObject;
                TopTransform = topTransform;
                MoveSpeed = moveSpeed;
                MaxHealth = maxHealth;
                PlayerHitPoint = playerHitPoint;
                SlimeData = slimeData;
                ShootingReqComponents = shootingReqComponents;
                ShootingSettings = shootingSettings;
                DeadSlimeSFX = deadSlimeSFX;
                SoulSprite = soulSprite;
            }
        }
        
        private GameObject _gameObject;
        private SlimeSideMovement _movement;
        private SlimeSideHealth _health;
        private SlimeData _slimeData;
        private SlimeAnimatorController _animatorController;
        private readonly SlimeSideShooting _shooter;
        private Renderer _soulSprite;

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
                _slimeData,
                format.DeadSlimeSFX
            ));

            _shooter = new SlimeSideShooting(this,
                format.ShootingSettings,
                format.ShootingReqComponents);
            
            _soulSprite = format.SoulSprite;
        }
        
        public Vector3 Position => _gameObject.transform.position;
        public Transform Transform => _gameObject.transform;
        public Transform TopPosition { get; private set; }
        public bool IsDead => _health.IsDead;

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

        public Animator Animator => _animatorController.Animator;

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

        public IEnumerator AnimateSideSwitch(Transform otherSlimeTransform, float duration)
        {
            StartChangeSides();

            float elapsed = 0f;
            while (elapsed < duration)
            {
                float t = elapsed / duration;
                _soulSprite.transform.position = Vector3.Lerp(_gameObject.transform.position, otherSlimeTransform.position, t);
                
                elapsed += Time.deltaTime;
                yield return null;
            }
            
            EndChangeSides(otherSlimeTransform.position);
        }

        public IEnumerator SnapToWater(Vector3 position, float duration)
        {
            float elapsed = 0f;
            Vector3 oldPosition = _gameObject.transform.position;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                Vector3 newPos = Vector3.Lerp(oldPosition, position, elapsed / duration);
                _gameObject.transform.position = newPos;
                yield return null;
            }
        }

        public void LockMovement()
        {
            _movement.LockMovement();
        }

        public void UnlockMovement()
        {
            _movement.UnlockMovement();
        }

        private void StartChangeSides()
        {
            _soulSprite.gameObject.SetActive(true);
            _animatorController.SetStartChange();
            _soulSprite.transform.position = _gameObject.transform.position;
        }

        private void EndChangeSides(Vector3 endPosition)
        {
            _soulSprite.transform.position = endPosition;
            _animatorController.SetEndChange(); 
        }
    }
}
