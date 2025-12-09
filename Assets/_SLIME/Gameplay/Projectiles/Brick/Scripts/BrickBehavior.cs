using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Player.Brick
{
    public class BrickBehavior: MonoBehaviour
    {
        [SerializeField] private float force = 20f;
        
        private Rigidbody2D _rb;
        private InputAction _shootAction;
        private bool _wasShot;
        private bool _wasCaught;
        
        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            
            _shootAction = new InputAction("Shoot");
            _shootAction.AddBinding("<Keyboard>/space");
            _shootAction.AddBinding("<Gamepad>/rightTrigger");
        }

        private void OnEnable()
        {
            _shootAction.performed += OnShoot;
            _shootAction.Enable();
            GameEvents.SlimeTears += Unfreeze;
        }

        private void Unfreeze()
        {
            if (_rb == null)
            {
                GameEvents.SlimeTears -= Unfreeze;
                return;
            }

            _rb.constraints = RigidbodyConstraints2D.None;
            GameEvents.SlimeTears -= Unfreeze;
        }

        private void OnDisable()
        {
            _shootAction.performed -= OnShoot;
            _shootAction.Disable();
            GameEvents.SlimeTears -= Unfreeze;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (_wasShot || _wasCaught) return;
            if (other.gameObject.CompareTag("Line"))
            {
                _wasCaught = true;
                _rb.constraints = RigidbodyConstraints2D.FreezeAll;
                Debug.Log("Brick catched!");
            }
        }
        
        private void OnShoot(InputAction.CallbackContext ctx)
        {
            if (!ctx.performed) return;
            
            if (!_wasShot && _wasCaught)
            {
                Shoot();
                gameObject.layer = LayerMask.NameToLayer("Player Projectiles");
            }
        }

        private void Shoot()
        {
            _wasShot = true;
            Unfreeze();
            _rb.AddForce(OldSlimeData.Instance().GetShotDirection(transform.position) * force, ForceMode2D.Impulse);
            GameEvents.BrickShot?.Invoke();
        }
    }
}