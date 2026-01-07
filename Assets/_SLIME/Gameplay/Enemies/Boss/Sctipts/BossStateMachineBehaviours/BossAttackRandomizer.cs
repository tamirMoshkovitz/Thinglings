using System;
using UnityEngine;
using System.Collections.Generic;
using Random = UnityEngine.Random;

public enum BossAttackType
{
    Smash,
    Spawn,
    Laser,
    Idle
}

namespace _SLIME.Boss
{
    public class BossAttackRandomizer : BossBaseBehaviour
    {
        
        [Header("Attacks Configurations")]
        [SerializeField] List<BossAttackType> availableAttacks;
        
        [Header("Hold Settings")] 
        public float duration = 1f;
        
        private static readonly int DoSmash = Animator.StringToHash("DoSmash");
        private static readonly int DoSpawn = Animator.StringToHash("DoSpawn");
        private static readonly int DoLaser = Animator.StringToHash("DoLaser");
        private static readonly int DoIdle = Animator.StringToHash("DoIdle");
        
        private float _timer;
        
        
        override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            base.OnStateUpdate(animator, stateInfo, layerIndex);
            _timer += Time.deltaTime;

            if (!(_timer >= duration)) return;
            int index = Random.Range(0, availableAttacks.Count);
            BossAttackType selectedAttack = availableAttacks[index];
            PreformSelectedAttack(animator, selectedAttack);
        }

        private void PreformSelectedAttack(Animator animator, BossAttackType selectedAttack)
        {
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
                case BossAttackType.Idle:
                    break;
                default:
                    animator.SetTrigger(DoIdle);
                    break;
            }
        }
    }
}