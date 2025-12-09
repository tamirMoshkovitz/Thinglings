using UnityEngine;
using UnityEngine.InputSystem;

namespace _SLIME.Gameplay.Slime.Scripts.new_scripts
{
    public class SlimeSideMovement
    {
        public struct SlimeSideMovementFormat
        {
            public SlimeSide Parent;
            public Rigidbody2D Rigidbody;
            public float MoveSpeed;
            
            public SlimeSideMovementFormat(SlimeSide parent, Rigidbody2D rigidbody, float moveSpeed)
            {
                Parent = parent;
                Rigidbody = rigidbody;
                MoveSpeed = moveSpeed;
            }
        }
        
        private SlimeSide _parent;
        private readonly float _moveSpeed;
        private readonly Rigidbody2D _rigidbody;
        
        private Vector2 _moveInput = Vector2.zero;
        
        public SlimeSideMovement(SlimeSideMovementFormat format)
        {
            _parent = format.Parent;
            _moveSpeed = format.MoveSpeed;
            _rigidbody = format.Rigidbody;
            _rigidbody.interpolation = RigidbodyInterpolation2D.Interpolate;
        }
        
        public void OnEnable() { }
        public void OnDisable() { }

        public void Update() { }
        
        public void FixedUpdate()
        {
            // Map input X -> velocity.x and input Y -> velocity.y so up/down respond
            Vector2 newVel = _moveInput * _moveSpeed;
            _rigidbody.linearVelocity = newVel;
        }
        
        public void OnMove(InputAction.CallbackContext context)
        {
            _moveInput = _parent.IsDead ? Vector2.zero : context.ReadValue<Vector2>();
        }
    }
}