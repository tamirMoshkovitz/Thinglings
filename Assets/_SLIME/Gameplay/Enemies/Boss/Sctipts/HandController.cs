using UnityEngine;
using DG.Tweening; 
using System.Collections;

public class HandController : MonoBehaviour
{
    [Header("References")]
    public Transform player;
    // Optional: Assign an empty GameObject located at the center of the arena
    public Transform centerBound; 

    [Header("Position Settings")]
    public float hoverY = 20f;     
    public float prepareY = 18f;  
    public float floorY = -1.5f;    
    
    [Header("Attack Settings")]
    public float entryDuration = 0.5f; 
    public float trackDuration = 2.0f;
    public float lockDuration = 0.5f; // Time to stop and "shake" before dropping
    public float trackSpeed = 10f;     
    public float dropDuration = 0.2f;  
    public float riseDuration = 1.0f;
    
    [Header("Camera Shake")]
    [SerializeField] private Camera mainCamera;
    [SerializeField] private float shakeDuration = 0.3f; // Reduced duration
    [SerializeField] private float shakeStrength = 0.3f; // Reduced intensity
    [SerializeField] private int shakeVibrato = 10;
    
    [Header("Hand Side")]
    [SerializeField] private bool isRightHand = false;

    private bool _isAttacking;
    private Tween _currentTween;

    private void Start()
    {
        // Ensure camera is assigned
        if (mainCamera == null) mainCamera = Camera.main;
    }

    public void StartAttack()
    {
        if(!_isAttacking) StartCoroutine(SmashRoutine());
    }

    IEnumerator SmashRoutine()
    {
        _isAttacking = true;

        // 1. ENTRY: Move to the ready height
        yield return transform.DOMoveY(prepareY, entryDuration).SetEase(Ease.OutBack).WaitForCompletion();

        // 2. TRACKING: Follow the player smoothly
        float timer = 0f;
        while (timer < trackDuration)
        {
            float currentX = transform.position.x;
            float targetX = player.position.x;

            // Apply Bounds Logic (Prevent crossing center)
            if (centerBound != null)
            {
                if (isRightHand)
                {
                    // Right hand stays on the Right side (Values > Bound)
                    targetX = Mathf.Max(player.position.x, centerBound.position.x);
                }
                else
                {
                    // Left hand stays on the Left side (Values < Bound)
                    targetX = Mathf.Min(player.position.x, centerBound.position.x);
                }
            }

            // Smoothly move X, keep Y locked at prepareY
            float newX = Mathf.Lerp(currentX, targetX, Time.deltaTime * trackSpeed);
            transform.position = new Vector3(newX, prepareY, transform.position.z);

            timer += Time.deltaTime;
            yield return null;
        }

        // 3. LOCK ON: Stop tracking and shake slightly (Telegraph the hit)
        // This gives the player a split second to dodge
        transform.DOShakePosition(lockDuration, 0.3f, 10, 90, false, true); 
        yield return new WaitForSeconds(lockDuration);

        // 4. DROP: Smash down
        yield return transform.DOMoveY(floorY, dropDuration).SetEase(Ease.InExpo).WaitForCompletion();

        // 5. IMPACT: Shake Camera
        if (mainCamera != null)
        {
            mainCamera.DOShakePosition(shakeDuration, shakeStrength, shakeVibrato);
        }
        
        // Wait on the floor for a moment?
        yield return new WaitForSeconds(0.5f);

        // 6. RECOVER: Move back up
        yield return transform.DOMoveY(hoverY, riseDuration).SetEase(Ease.InOutSine).WaitForCompletion();

        _isAttacking = false;
    }

    // Safety cleanup
    private void OnDisable()
    {
        transform.DOKill();
    }
}