using UnityEngine;
using UnityEngine.InputSystem;

namespace _SLIME.Slime
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

        private Vector2 MoveInput
        {
            get => _parent.IsDead? Vector2.zero : _moveInput;
            set => _moveInput = value;
        }
        
        public SlimeSideMovement(SlimeSideMovementFormat format)
        {
            _parent = format.Parent;
            _moveSpeed = format.MoveSpeed;
            _rigidbody = format.Rigidbody;
            _rigidbody.interpolation = RigidbodyInterpolation2D.Interpolate;
        }

        public bool IsMoving => _rigidbody.linearVelocity.magnitude > .1f;
        
        public void OnEnable() { }
        public void OnDisable() { }

        public void Update() { }
        
        public void FixedUpdate()
        {
            Vector2 newVel = MoveInput * _moveSpeed;
            _rigidbody.linearVelocity = newVel;
        }
        
        public void OnMove(InputAction.CallbackContext context)
        {
            MoveInput = _parent.IsDead ? Vector2.zero : context.ReadValue<Vector2>();
        }
    }
}