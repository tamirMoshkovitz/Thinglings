using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;

    private Rigidbody2D _rb;
    private Vector2 _moveInput;
    [SerializeField] private Vector3 originalTransform;
    [SerializeField] private LayerMask handLayer;
    [SerializeField] private LayerMask floorLayer;
    [SerializeField] private GameObject slimeEyes;
    
    public bool MovementLocked { get; set; } = false;
    

    private void Start()
    {
        _rb = GetComponent<Rigidbody2D>();
        _rb.interpolation = RigidbodyInterpolation2D.Interpolate;
    }

    private void OnEnable()
    {
        GameEvents.slimeConnected += OnSlimeConnected;
    }
    
    private void OnDisable()
    {
        GameEvents.slimeConnected -= OnSlimeConnected;
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        if (MovementLocked) return;
        _moveInput = MovementLocked ? Vector2.zero : context.ReadValue<Vector2>();
    }

    public void OnReset(InputAction.CallbackContext context)
    {
        if (!context.performed) return;
        GameEvents.RaiseResetGame();
        gameObject.transform.position = new Vector3(originalTransform.x, originalTransform.y, originalTransform.z);
    }

    private void FixedUpdate()
    {
        // Map input X -> velocity.x and input Y -> velocity.y so up/down respond
        Vector2 newVel = _moveInput * moveSpeed;
        _rb.linearVelocity = newVel;
        
        if (!MovementLocked)
        {
            CheckIfSmashed();
        }
    }

    private void CheckIfSmashed()
    {
        // raycast up and down to check hand from above and floor from below
        RaycastHit2D hitUp = Physics2D.Raycast(transform.position, Vector2.up * 1.5f, handLayer);
        RaycastHit2D hitDown = Physics2D.Raycast(transform.position, Vector2.down * 1.5f, floorLayer);
        
        Debug.DrawRay(transform.position, Vector2.up * 1.5f, Color.red);
        Debug.DrawRay(transform.position, Vector2.down * 1.5f, Color.blue);
        
        // if (hitUp.collider && hitDown.collider && hitUp.collider.CompareTag("Hand") && hitDown.collider.CompareTag("Wall"))
        if (hitUp.collider && hitUp.collider.CompareTag("Hand"))
        {
            MovementLocked = true;
            _moveInput = Vector2.zero;
            slimeEyes.SetActive(false);
        }
    }
    
    private void OnSlimeConnected()
    {
        MovementLocked = false;
        slimeEyes.SetActive(true);
    }
}