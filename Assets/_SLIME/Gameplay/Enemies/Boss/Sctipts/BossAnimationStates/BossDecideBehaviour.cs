using UnityEngine;
using System.Collections.Generic;


namespace _SLIME.Boss
{
    public class BossDecideBehaviour : BossBaseBehaviour
    {
        public enum BossAttackType
        {
            Smash,
            Spawn,
            Laser
        }

        [Header("Configuration")]
        // Add the specific attacks you want this boss to be able to use here
        public List<BossAttackType> availableAttacks;

        // Cache hashes for performance
        private static readonly int DoSmash = Animator.StringToHash("DoSmash");
        private static readonly int DoSpawn = Animator.StringToHash("DoSpawn");
        private static readonly int DoLaser = Animator.StringToHash("DoLaser");

        override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            base.OnStateEnter(animator, stateInfo, layerIndex);

            if (availableAttacks.Count == 0) return;

            int index = Random.Range(0, availableAttacks.Count);
            BossAttackType selectedAttack = availableAttacks[index];

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
            }
        }
    }
}