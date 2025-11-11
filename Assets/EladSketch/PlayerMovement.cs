using System;
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace EladSketch
{
    public class PlayerMovement: MonoBehaviour
    {
        [SerializeField] private Rigidbody2D rb;
        [SerializeField] private CircleCollider2D col;
        [SerializeField] private Camera cam;
        
        [Tooltip("Controls how much can the player stretch the player from initial position")]
        [SerializeField] [Range(1, 50)] private float stretchRange;
        
        [Tooltip("Max Speed of Player")]
        [SerializeField] [Range(1, 200)] private float maxSpeed;
        
        
        [Tooltip("Controls how much the stretch of the player effects the speed")]
        [SerializeField] [Range(0, 5)] private float stretchMagnifier;
        
        [Tooltip("Controls the change in direction of the player")]
        [SerializeField] [Range(-1, 1)]private float minDotForChange;

        [SerializeField] [CanBeNull] private Collider2D[] walls = new Collider2D[5];
        
        [SerializeField] private LayerMask wallLayer; 
        private ContactFilter2D wallFilter;

        [SerializeField] private LayerMask playerLayer;

        [SerializeField]
        [Range(1, 200)] private float baseSpeed;
        
        private float _currentSpeed;
        
        private EladInputSystem _inputSystem;
        private bool _isDragging;
        private Vector2 _targetPosition;
        private RigidbodyType2D _originalBodyType;
        private Vector2 _direction;
        [SerializeField] private bool log;
        private Vector2 _lastCheckedPosition;
        [SerializeField] [Range(0,3)] private float checkDirectionBuffer;
        private Vector2 _currentDirection;
        [SerializeField] [Range(0,20)] private float bufferSpeedCollision;


        private void Awake()
        {
            Debug.Log("Awake");
            _inputSystem = new EladInputSystem();
            wallFilter.useTriggers = false;
            wallFilter.SetLayerMask(wallLayer);
            wallFilter.useLayerMask = true;
            
        }

        private void OnEnable()
        {
            _inputSystem.Player.LeftClick.Enable();
            _inputSystem.Player.LeftClick.started += OnClickStarted;
            _inputSystem.Player.LeftClick.canceled += OnClickCanceled;
        }

        private void OnDisable()
        {
            _inputSystem.Player.LeftClick.Disable();
            _inputSystem.Player.LeftClick.started -= OnClickStarted;
            _inputSystem.Player.LeftClick.canceled -= OnClickCanceled;
        }

        private void Update()
        {
            
        }

        private void FixedUpdate()
        {
            if (_isDragging)
            {
                var collision = Physics2D.OverlapCircle(
                    (Vector2)_targetPosition + col.offset, 
                    col.radius,                              
                    wallLayer                                
                );                      
                if (collision == null)
                {
                    rb.MovePosition(_targetPosition);
                }
            }
        }
        
        


        private void OnClickStarted(InputAction.CallbackContext ctx)
        {
            Vector2 screenPos = Mouse.current.position.ReadValue();
            Vector3 worldPos = cam.ScreenToWorldPoint(screenPos);
            var hit = Physics2D.Raycast(worldPos, Vector2.zero, 0f, playerLayer);
            if (hit.collider == null) return;
            _lastCheckedPosition = transform.position;
            _targetPosition = new Vector2(worldPos.x, worldPos.y);
            _direction = (new Vector2(_targetPosition.x,_targetPosition.y ) - _lastCheckedPosition).normalized;
            _currentDirection = _direction;
            _isDragging = true;
            _originalBodyType = rb.bodyType;
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
            StartCoroutine(TrackMouse());
        }

        private void OnClickCanceled(InputAction.CallbackContext ctx)
        {
            if(!_isDragging) return;
            LaunchPlayer();
        }

        private void LaunchPlayer()
        {
            _isDragging = false;
            var launchDirection = (Vector2)transform.position - _lastCheckedPosition; 
            var stretch = launchDirection.magnitude; 
            var speed = Mathf.Min(baseSpeed * (1 + stretch * stretchMagnifier), maxSpeed);
            launchDirection = -launchDirection.normalized;
            rb.bodyType = _originalBodyType;
            if(log) Debug.Log(speed);
            // rb.AddForce(-_direction * speed, ForceMode2D.Impulse);
            rb.linearVelocity = speed * launchDirection;
            _direction = launchDirection;
        }

        private IEnumerator TrackMouse()
        {
            float zDistance = Mathf.Abs(cam.transform.position.z - transform.position.z);
            StartCoroutine(CheckDirection());
            while (_isDragging)
            {
                Vector2 screenPos = Mouse.current.position.ReadValue();
                Vector3 worldPos = cam.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, zDistance));
                worldPos.z = transform.position.z;
                _targetPosition = new Vector2(worldPos.x, worldPos.y);
                
                float dotProduct = Vector2.Dot(_direction, _currentDirection);
                if (dotProduct < minDotForChange)
                {
                    if(log) Debug.Log(_direction.x + "," + _direction.y + "," + _currentDirection.x + "," + _currentDirection.y);
                    if(log) Debug.Log($"Changing direction! Dot was: {dotProduct}");
                    _direction = _currentDirection;
                    _lastCheckedPosition = transform.position;
                }
                yield return null; 
            }
        }

        private IEnumerator CheckDirection()
        {
            while (_isDragging)
            {
                var lastPosition = transform.position;
                yield return new WaitForSeconds(0.5f);
                if((lastPosition - transform.position).magnitude > checkDirectionBuffer ) 
                    _currentDirection = (new Vector2(_targetPosition.x,_targetPosition.y) - (Vector2)lastPosition).normalized;
            }
        }


        private void OnCollisionEnter2D(Collision2D collision)
        {
            
            _isDragging = false;
            if (collision.collider.CompareTag("FlipX") && rb.linearVelocity.magnitude > bufferSpeedCollision)
            {
                Debug.Log($"Collision entered: {collision.gameObject.name}");
                Vector2 currentVelocity = rb.linearVelocity;
                Vector2 newVelocity = new Vector2(-currentVelocity.x, currentVelocity.y);
                rb.linearVelocity = newVelocity;
            }
            else if (collision.collider.CompareTag("FlipY") && rb.linearVelocity.magnitude > bufferSpeedCollision)
            {
                Debug.Log($"Collision entered: {collision.gameObject.name}");
                Vector2 currentVelocity = rb.linearVelocity;
                Vector2 newVelocity = new Vector2(currentVelocity.x, -currentVelocity.y);
                rb.linearVelocity = newVelocity;
            }
        }
    }
}