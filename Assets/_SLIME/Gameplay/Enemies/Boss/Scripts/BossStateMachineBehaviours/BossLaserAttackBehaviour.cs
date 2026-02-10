using UnityEngine;
using System.Collections;
using _SLIME.Laser;
using Unity.VisualScripting;

namespace _SLIME.Boss
{
    public class BossLaserAttackBehaviour : BossBaseBehaviour
    {
        private static readonly int AttackFinished = Animator.StringToHash("AttackFinished");
        private LaserAttackLogic laserAttackLogic;
        private GameObject _laser;
        
        override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            base.OnStateEnter(animator, stateInfo, layerIndex);
            Data.BossLaserState();
            TotalAttacksPreformed++;
            _laser = BossCheckpointManager.Instance.CurrentSavedPhase ==
                    BossPhaseType.FirstPhase ? Data.laserAttackGameObjectPhase1 : Data.laserAttackGameObjectPhase2;
            _laser.SetActive(true);
            laserAttackLogic = _laser.GetComponent<LaserAttackLogic>();
        }

        override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            if (Data.WaterStateActivated || laserAttackLogic.HasFinishedAction)
            {
                animator.SetTrigger(AttackFinished);
                _laser.SetActive(false);
            }
        }
        
    }
}