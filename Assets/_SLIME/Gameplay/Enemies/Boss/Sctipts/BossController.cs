using System;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System.Collections;
using Random = UnityEngine.Random;

public class BossController : MonoBehaviour
{
    [Header("Health Settings")]
    public float maxHealth = 100f;
    public Image bossHealthBar;
    private float _currentHealth;
    private bool _isDead;

    [Header("Eye References")]
    public Transform leftEyeRoot;
    public Collider2D leftCollider;
    public EyeFollow leftTracker;
    public Transform rightEyeRoot;
    public Collider2D rightCollider;
    public EyeFollow rightTracker;
    
    [Header("Attacks")]
    public HandController leftHand;  
    public HandController rightHand; 
    public GameObject fallingItemPrefab; 
    public Transform spawnAreaLeft;
    public Transform spawnAreaRight;

    [Header("Eyes Movement & Timing")]
    public float verticalOffset = 15f;
    public float moveSpeed = 1f;
    public float timeOpen = 5f;
    public float timeClosed = 2f;

    [Header("Attacks Speed")] 
    public float spawnSpeed = 3f;
    public float smashInterval = 1.5f;  
    
    private Tween _leftHoverTween;
    private Tween _rightHoverTween;
    private Coroutine _currentAttackRoutine; 
    
    // NEW: We need to remember where the eyes started to reset them correctly
    private Vector3 _initialLeftPos;
    private Vector3 _initialRightPos;
    
    void Start()
    {
        // 1. Setup Health
        _currentHealth = maxHealth;
        if (bossHealthBar != null) bossHealthBar.fillAmount = 1f;

        // 2. Setup Eyes
        PrepareEye(leftEyeRoot, leftCollider, true);
        PrepareEye(rightEyeRoot, rightCollider, false);

        // 3. Start Loop
        StartCoroutine(BossRoutine());
    }

    private void OnEnable()
    {
        GameEvents.ResetButtonPressed += OnResetButtonPressed;
    }

    private void OnDisable()
    {
        GameEvents.ResetButtonPressed -= OnResetButtonPressed;
    }

    // Updated to cache positions
    void PrepareEye(Transform eye, Collider2D col, bool isLeft)
    {
        // Save the position BEFORE we move it up, so we have a reference point
        if (isLeft) _initialLeftPos = eye.position; 
        else _initialRightPos = eye.position;

        // Move it up to the hidden position
        eye.position += Vector3.up * verticalOffset;
        col.enabled = false;
    }

    IEnumerator BossRoutine()
    {
        yield return new WaitForSeconds(1f);

        // Recalculate targets based on the current (Hidden) position
        Vector3 leftTarget = leftEyeRoot.position - (Vector3.up * verticalOffset);
        Vector3 rightTarget = rightEyeRoot.position - (Vector3.up * verticalOffset);

        while (!_isDead)
        {
            MoveEyeDown(leftEyeRoot, leftTarget, leftCollider, leftTracker, true);
            MoveEyeDown(rightEyeRoot, rightTarget, rightCollider, rightTracker, false);

            float choice = Random.value; // Fixed: Random.value returns 0.0 to 1.0
            
            _currentAttackRoutine = StartCoroutine(choice < 0.5f ? SpawnItemsRoutine() : MultiSmashRoutine());

            yield return new WaitForSeconds(moveSpeed + timeOpen);

            if (_currentAttackRoutine != null) 
            {
                StopCoroutine(_currentAttackRoutine);
                _currentAttackRoutine = null;
            }
            
            HideEye(leftEyeRoot, leftTarget + (Vector3.up * verticalOffset), leftCollider, leftTracker, true);
            HideEye(rightEyeRoot, rightTarget + (Vector3.up * verticalOffset), rightCollider, rightTracker, false);

            yield return new WaitForSeconds(moveSpeed + timeClosed);
        }
    }

    IEnumerator SpawnItemsRoutine()
    {
        while (true)
        {
            float randomX = Random.Range(spawnAreaLeft.position.x, spawnAreaRight.position.x);
            float fixedY = spawnAreaLeft.position.y;
            GameObject item = Instantiate(fallingItemPrefab, new Vector2(randomX, fixedY), Quaternion.identity);
            
            // Optional: Destroy item after 5 seconds so they don't pile up forever
            Destroy(item, 5f); 
            
            yield return new WaitForSeconds(spawnSpeed); 
        }
    }

    IEnumerator MultiSmashRoutine()
    {
        yield return new WaitForSeconds(0.5f);
        while (true)
        {
            if (Random.value > 0.5f) leftHand.StartAttack();
            else rightHand.StartAttack();

            yield return new WaitForSeconds(smashInterval);
        }
    }

    void MoveEyeDown(Transform eye, Vector3 targetPos, Collider2D col, EyeFollow tracker, bool isLeft)
    {
        col.enabled = true;      
        tracker.enabled = true;  
        eye.DOMove(targetPos, moveSpeed).SetEase(Ease.OutBack).OnComplete(() => 
        {
            Tween hover = eye.DOMoveY(targetPos.y + 0.5f, 1f).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.InOutSine);
            if (isLeft) _leftHoverTween = hover; else _rightHoverTween = hover;
        });
    }

    void HideEye(Transform eye, Vector3 hiddenPos, Collider2D col, EyeFollow tracker, bool isLeft)
    {
        if (isLeft && _leftHoverTween != null) _leftHoverTween.Kill();
        if (!isLeft && _rightHoverTween != null) _rightHoverTween.Kill();
        eye.DOMove(hiddenPos, moveSpeed).SetEase(Ease.InBack).OnComplete(() => { col.enabled = false; });
    }

    public void TakeDamage(float amount)
    {
        if (_isDead) return;
        _currentHealth -= amount;
        if(bossHealthBar != null) bossHealthBar.fillAmount = _currentHealth / maxHealth;
        if (_currentHealth <= 0) Die();
    }

    void Die()
    {
        _isDead = true;
        StopAllAttackCoroutines(); // Helper method
        
        KillEyeTweens(); // Helper method

        leftEyeRoot.DOScale(Vector3.zero, 0.5f);
        rightEyeRoot.DOScale(Vector3.zero, 0.5f);
        leftCollider.enabled = false;
        rightCollider.enabled = false;
    }

    // --- RESET LOGIC ---

    public void OnResetButtonPressed()
    {
        // 1. Stop Everything
        StopAllCoroutines();
        StopAllAttackCoroutines();
        KillEyeTweens();

        // 2. Reset Variables
        _isDead = false;
        _currentHealth = maxHealth;
        if (bossHealthBar != null) bossHealthBar.fillAmount = 1f;

        // 3. Reset Transforms (Snap them back to the hidden position immediately)
        // Note: _initialPos is the "Low" position, so we add verticalOffset to get "High"
        leftEyeRoot.position = _initialLeftPos + (Vector3.up * verticalOffset);
        rightEyeRoot.position = _initialRightPos + (Vector3.up * verticalOffset);
        
        leftEyeRoot.localScale = Vector3.one;
        rightEyeRoot.localScale = Vector3.one;

        // 4. Reset Components
        leftCollider.enabled = false;
        rightCollider.enabled = false;
        leftTracker.enabled = false;
        rightTracker.enabled = false;

        // 5. Cleanup scene (Optional: Destroy existing falling items)
        var items = GameObject.FindGameObjectsWithTag($"FallingItem"); // Ensure prefab has this tag
        foreach (var item in items) Destroy(item);

        // 6. Restart the logic
        StartCoroutine(BossRoutine());
    }

    // --- HELPER FUNCTIONS ---
    
    private void StopAllAttackCoroutines()
    {
        if (_currentAttackRoutine != null) 
        {
            StopCoroutine(_currentAttackRoutine);
            _currentAttackRoutine = null;
        }
    }

    private void KillEyeTweens()
    {
        // Kill specific hover tweens
        if (_leftHoverTween != null) _leftHoverTween.Kill();
        if (_rightHoverTween != null) _rightHoverTween.Kill();
        
        // Kill any movement tweens directly on the transforms
        leftEyeRoot.DOKill();
        rightEyeRoot.DOKill();
    }
}