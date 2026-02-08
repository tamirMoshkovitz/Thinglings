using System;
using UnityEngine;
using System.Collections.Generic;
using _SLIME.GameLoop;
using Random = UnityEngine.Random;

public enum BossAttackType
{
    Smash,
    Spawn,
    Laser,
    Idle,
    LightHouse,
    LittleBosses
}

namespace _SLIME.Boss
{
    public class BossAttackRandomizer : BossBaseBehaviour
    {
        [Header("Attacks Per Phase")]
        [SerializeField] List<BossAttackType> firstPhaseAttacks;
        [SerializeField] List<BossAttackType> secondPhaseAttacks;
        [SerializeField] List<BossAttackType> tunnelPhaseAttacks;
        
        [Header("Hold Settings")] 
        private static readonly int DoSmash = Animator.StringToHash("DoSmash");
        private static readonly int DoSpawn = Animator.StringToHash("DoSpawn");
        private static readonly int DoLaser = Animator.StringToHash("DoLaser");
        private static readonly int DoIdle = Animator.StringToHash("DoIdle");
        private static readonly int DoWater = Animator.StringToHash("DoWater");
        private static readonly int DoLightHouse = Animator.StringToHash("DoLightHouse");
        private static readonly int DoLittleBosses = Animator.StringToHash("DoLittleBosses");
        
        private Dictionary<BossAttackType, float> _lastUsedTime = new();
        
        
        override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            base.OnStateEnter(animator, stateInfo, layerIndex);
            
            
            List<BossAttackType> availableAttacks = GetAttacksForCurrentState();
            if (availableAttacks == null || availableAttacks.Count == 0) return;
            
            BossAttackType selectedAttack = SelectLeastRecentlyUsed(availableAttacks);
            _lastUsedTime[selectedAttack] = Time.time;
            PreformSelectedAttack(animator, selectedAttack);
        }

      
        private BossAttackType SelectLeastRecentlyUsed(List<BossAttackType> attacks)
        {
            if (attacks.Count == 1) return attacks[0];
            
            float oldestTime = _lastUsedTime.TryGetValue(attacks[0], out float t) ? t : float.MinValue;
            var candidates = new List<BossAttackType> { attacks[0] };
            for (int i = 1; i < attacks.Count; i++)
            {
                float lastTime = _lastUsedTime.TryGetValue(attacks[i], out float lt) ? lt : float.MinValue;
                if (lastTime < oldestTime)
                {
                    oldestTime = lastTime;
                    candidates.Clear();
                    candidates.Add(attacks[i]);
                }
                else if (lastTime == oldestTime)
                {
                    candidates.Add(attacks[i]);
                }
            }
            return candidates[Random.Range(0, candidates.Count)];
        }
        
        private List<BossAttackType> GetAttacksForCurrentState()
        {
            if (Data == null) return null;
            return Data.CurrentPhase switch
            {
                BossPhaseType.FirstPhase => firstPhaseAttacks,
                BossPhaseType.SecondPhase => secondPhaseAttacks,
                BossPhaseType.TunnelPhase => tunnelPhaseAttacks,
                _ => firstPhaseAttacks
            };
        }

        private void PreformSelectedAttack(Animator animator, BossAttackType selectedAttack)
        {
            if (Data.WaterStateActivated)
            {
                animator.SetTrigger(DoWater);
                Debug.Log("Water Attack Activated");
                GameEvents.WaterAttackStarted?.Invoke();
                return;
            }
            switch (selectedAttack)
            {
                case BossAttackType.Smash:
                    animator.SetTrigger(DoSmash);
                    break;
                case BossAttackType.Spawn:
                    animator.SetTrigger(DoSpawn);
                    break;
                case BossAttackType.Laser:
                    animator.SetTrigger(DoLaser);
                    break;
                case BossAttackType.LightHouse:
                    animator.SetTrigger(DoLightHouse);
                    break;
                case BossAttackType.LittleBosses:
                    animator.SetTrigger(DoLittleBosses);
                    break;
                case BossAttackType.Idle:
                    break;
                default:
                    animator.SetTrigger(DoIdle);
                    break;
            }
        }
    }
}