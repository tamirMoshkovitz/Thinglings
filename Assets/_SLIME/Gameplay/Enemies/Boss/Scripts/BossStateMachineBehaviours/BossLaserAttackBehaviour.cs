using UnityEngine;
using System.Collections;
using _SLIME.Laser;

namespace _SLIME.Boss
{
    public class BossLaserAttackBehaviour : BossBaseBehaviour
    {
        private static readonly int AttackFinished = Animator.StringToHash("AttackFinished");
        private LaserAttackLogic laserAttackLogic;
        
        override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            base.OnStateEnter(animator, stateInfo, layerIndex);
            Data.BossLaserState();
            TotalAttacksPreformed++;
            Data.laserAttackGameObject.SetActive(true);
            laserAttackLogic = Data.laserAttackGameObject.GetComponent<LaserAttackLogic>();
        }

        override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            if (!laserAttackLogic.HasFinishedAction) return;
            Data.laserAttackGameObject.SetActive(false);
            animator.SetTrigger(AttackFinished);
        }
        
    }
}