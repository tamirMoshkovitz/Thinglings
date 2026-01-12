using System;
using _SLIME.Boss;
using UnityEngine;

public class WaterStateBrain : MonoBehaviour
{
    private static readonly int SlimeWon = Animator.StringToHash("SlimeWon");
    
    [SerializeField] private WaterAttackBehaviour LeftWaterAttackBehaviour;
    [SerializeField] private WaterAttackBehaviour RightWaterAttackBehaviour;
    [SerializeField] private BossBrain bossBrain;
    [SerializeField] private Animator bossAnimator;
    
    [Header("Settings")]
    [SerializeField] private float damageCooldown = 10f;

    private float _nextAllowedDamageTime = 0f;

    public void Update()
    {
        bool isCooldownFinished = Time.time >= _nextAllowedDamageTime;

        if (LeftWaterAttackBehaviour._canAttack && 
            RightWaterAttackBehaviour._canAttack && 
            isCooldownFinished)
        {
            bossAnimator.SetTrigger(SlimeWon);
            bossBrain.TakeDamage(20.0f);
            
            _nextAllowedDamageTime = Time.time + damageCooldown;
        }
    }
}