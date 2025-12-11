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

    [Header("Boss References")]
    public Transform bossRoot;       // The main parent object of the enemy
    public Collider2D bossCollider;  // The main collider for taking damage
    
    [Header("Attacks")]
    public HandController leftHand;  
    public HandController rightHand; 
    public GameObject fallingItemPrefab; 
    public Transform spawnAreaLeft;
    public Transform spawnAreaRight;

    [Header("Movement & Timing")]
    public float verticalOffset = 15f;
    public float moveSpeed = 1f;
    public float timeOpen = 5f;
    public float timeClosed = 2f;

    [Header("Attacks Speed")] 
    public float spawnSpeed = 3f;
    public float smashInterval = 1.5f;  
    
    private Tween _bossHoverTween;
    private Coroutine _currentAttackRoutine; 
    
    private Vector3 _attackPos;
    private Vector3 _hiddenPos;
    
    void Start()
    {
        _currentHealth = maxHealth;
        if (bossHealthBar != null) bossHealthBar.fillAmount = 1f;

        _attackPos = bossRoot.position;
        _hiddenPos = _attackPos + (Vector3.up * verticalOffset);

        PrepareBossInitially();

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

    void PrepareBossInitially()
    {
        bossRoot.position = _hiddenPos;
        bossCollider.enabled = false;
    }

    IEnumerator BossRoutine()
    {
        yield return new WaitForSeconds(1f);

        while (!_isDead)
        {
            MoveBossDown();

            float choice = Random.value;
            _currentAttackRoutine = StartCoroutine(choice < 0.5f ? SpawnItemsRoutine() : MultiSmashRoutine());

            yield return new WaitForSeconds(moveSpeed + timeOpen);

            if (_currentAttackRoutine != null) 
            {
                StopCoroutine(_currentAttackRoutine);
                _currentAttackRoutine = null;
            }
            
            // --- PHASE 3: HIDE ---
            HideBoss();

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

    void MoveBossDown()
    {
        bossCollider.enabled = true;
        
        bossRoot.DOMove(_attackPos, moveSpeed).SetEase(Ease.OutBack).OnComplete(() => 
        {
            // Hover animation while vulnerable
            _bossHoverTween = bossRoot.DOMoveY(_attackPos.y + 0.5f, 1f)
                .SetLoops(-1, LoopType.Yoyo)
                .SetEase(Ease.InOutSine);
        });
    }

    void HideBoss()
    {
        if (_bossHoverTween != null) _bossHoverTween.Kill();
        
        bossRoot.DOMove(_hiddenPos, moveSpeed).SetEase(Ease.InBack).OnComplete(() => 
        { 
            bossCollider.enabled = false;
        });
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
        StopAllAttackCoroutines(); 
        KillTweens();

        bossRoot.DOScale(Vector3.zero, 0.5f);
        bossCollider.enabled = false;
    }

    // --- RESET LOGIC ---

    public void OnResetButtonPressed()
    {
        StopAllCoroutines();
        StopAllAttackCoroutines();
        KillTweens();

        _isDead = false;
        _currentHealth = maxHealth;
        if (bossHealthBar != null) bossHealthBar.fillAmount = 1f;

        bossRoot.position = _hiddenPos;
        bossRoot.localScale = Vector3.one;

        bossCollider.enabled = false;

        var items = GameObject.FindGameObjectsWithTag($"FallingItem"); 
        foreach (var item in items) Destroy(item);

        StartCoroutine(BossRoutine());
    }

    private void StopAllAttackCoroutines()
    {
        if (_currentAttackRoutine != null) 
        {
            StopCoroutine(_currentAttackRoutine);
            _currentAttackRoutine = null;
        }
    }

    private void KillTweens()
    {
        if (_bossHoverTween != null) _bossHoverTween.Kill();
        bossRoot.DOKill();
    }
}