using UnityEngine;
using System.Collections;

namespace _SLIME.Boss
{
    public class BossLaserAttackBehaviour : BossBaseBehaviour
    {
        private static readonly int AttackFinished = Animator.StringToHash("AttackFinished");
        
        // Timer variables
        private float _timer;
        private float _duration;

        override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            base.OnStateEnter(animator, stateInfo, layerIndex);

            if (Data.laserAttackGameObject != null)
            {
                // 1. Turn ON
                Data.laserAttackGameObject.SetActive(true);

                // 2. Setup Timer
                _timer = 0f;
                // Calculate total duration needed
                _duration = Data.bossConfigurations.LaserAttack.rotationDuration + 1f;
            }
        }

        override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            // 3. Count up
            _timer += Time.deltaTime;

            // 4. Check if time is up
            if (_timer >= _duration)
            {
                // Turn OFF
                if (Data.laserAttackGameObject != null)
                {
                    Data.laserAttackGameObject.SetActive(false);
                }

                // Trigger Exit
                animator.SetTrigger(AttackFinished);
            }
        }

        override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            // Safety cleanup just in case
            if (Data.laserAttackGameObject != null)
            {
                Data.laserAttackGameObject.SetActive(false);
            }
        }
    }
}