using System;
using System.Collections;
using _SLIME.Boss;
using _SLIME.GameLoop;
using UnityEngine;

public class WaterAttackManager : MonoBehaviour
{
    private static readonly int CreaturesInsideTrigger = Animator.StringToHash("Creatures inside");
    private static readonly int AttackModeTrigger = Animator.StringToHash("Attack mode");
    private static readonly int MagicalWaterOutTrigger = Animator.StringToHash("Magical water out");
    private static readonly int SlimeWon = Animator.StringToHash("SlimeWon");
    private static readonly int BossWon = Animator.StringToHash("BossWon");

    [Header("Settings")]
    [SerializeField] private float initialDelay = 1f;
    [SerializeField] private float timeToAttackMode = 2f;
    [SerializeField] private float timeToMagicOut = 1f;
    private float waterAttackDamage => BossBrain.bossConfigurations.WaterAttack.waterDamage;

    [Header("References")]
    [SerializeField] private Animator animatorLeft;
    [SerializeField] private Animator animatorRight;
    [SerializeField] private BossBrain bossBrain;

    private bool _isLeftZoneActive;
    private bool _isRightZoneActive;
    private int waterAttackResult = BossWon;
    
    private Coroutine _attackRoutine;
    public bool CanAttack { get; private set; }

    public void SetZoneState(int sensorId, bool isActive)
    {
        if (sensorId == 0) _isLeftZoneActive = isActive;
        else if (sensorId == 1) _isRightZoneActive = isActive;

        CheckSynchronization();
    }

    private void OnEnable()
    {
        TunnelPhaseState.BossDead += OnBossDead;
    }
    
    private void OnDisable()
    {
        TunnelPhaseState.BossDead -= OnBossDead;
    }

    private void CheckSynchronization()
    {
        bool bothActive = _isLeftZoneActive && _isRightZoneActive;

        if (bothActive)
        {
            if (_attackRoutine == null)
            {
                _attackRoutine = StartCoroutine(AttackSequence());
            }
        }
        else
        {
            if (_attackRoutine != null)
            {
                StopCoroutine(_attackRoutine);
                _attackRoutine = null;
                GameEvents.WaterAttackEnded?.Invoke();
            }
            
            CanAttack = false;
        }
    }

    private IEnumerator AttackSequence()
    {
        Debug.Log("water attack started");
        
        yield return new WaitForSeconds(initialDelay);
        
        GameEvents.FmodPhaseFour?.Invoke();
        TriggerBoth(CreaturesInsideTrigger);

        yield return new WaitForSeconds(timeToAttackMode);
        TriggerBoth(AttackModeTrigger);
        CanAttack = true;
        
        yield return new WaitForSeconds(timeToMagicOut);
        TriggerBoth(MagicalWaterOutTrigger);

        _attackRoutine = null;
        GameEvents.WaterAttackEnded?.Invoke();
        Debug.Log("Water Attack Ended");
    }

    private void TriggerBoth(int hashId)
    {
        if(animatorLeft) animatorLeft.SetTrigger(hashId);
        if(animatorRight) animatorRight.SetTrigger(hashId);
        if (hashId != AttackModeTrigger) return;
        bossBrain.ApplyDamage(waterAttackDamage);
        bossBrain.animator.SetTrigger(waterAttackResult);
    }

    private void OnBossDead()
    {
        waterAttackResult = SlimeWon;
        GameEvents.SlimeWon?.Invoke();
    }
}