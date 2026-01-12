using System;
using Unity.VisualScripting;
using UnityEngine;

public class WaterAttackBehaviour : MonoBehaviour
{
    [SerializeField] private float stayDurationToStartAttack = 0f;
    [SerializeField] private Animator animator;
    private float _stayTimer;
    private bool _isPlayerInWaterZone;
    public bool _canAttack;
    
    public bool IsPlayerInWaterZone => _isPlayerInWaterZone;
    
    private void OnTriggerStay2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        _isPlayerInWaterZone = true;
        _stayTimer += Time.deltaTime;

        if (_stayTimer >= stayDurationToStartAttack)
        {
            animator.SetTrigger("Creatures inside");
            if (_stayTimer >= stayDurationToStartAttack + 2f)
            {
                animator.SetTrigger("Attack mode");
                _canAttack = true;
            }
            if (_stayTimer >= stayDurationToStartAttack + 3f)
            {
                animator.SetTrigger("Magical water out");
            }
        }
    }
    
    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        _isPlayerInWaterZone = false;
        _stayTimer = 0.0f;
        _canAttack = false;
    }
}
