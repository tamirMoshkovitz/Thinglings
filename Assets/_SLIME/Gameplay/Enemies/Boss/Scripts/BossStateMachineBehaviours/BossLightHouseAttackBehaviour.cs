using UnityEngine;
using _SLIME.LightHouse;

namespace _SLIME.Boss
{
    public class BossLightHouseAttackBehaviour : BossBaseBehaviour
    {
        private static readonly int AttackFinished = Animator.StringToHash("AttackFinished");
        private LightHouseAttackLogic lightHouseAttackLogic;
        
        override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            base.OnStateEnter(animator, stateInfo, layerIndex);
            Data.BossLaserState();
            TotalAttacksPreformed++;
            Data.lightHouseAttackGameObject.SetActive(true);
            lightHouseAttackLogic = Data.lightHouseAttackGameObject.GetComponent<LightHouseAttackLogic>();
        }

        override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            if (Data.WaterStateActivated || lightHouseAttackLogic.HasFinishedAction)
            {
                animator.SetTrigger(AttackFinished);
                Data.lightHouseAttackGameObject.SetActive(false);
            }
        }
        
    }
}