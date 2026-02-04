using UnityEngine;
using _SLIME.LightHouse;

namespace _SLIME.Boss
{
    public class BossLightHouseAttackBehaviour : BossBaseBehaviour
    {
        private static readonly int AttackFinished = Animator.StringToHash("AttackFinished");
        private LightHouseAttackLogic lightHouseAttackLogic;
        private float _firstFloatDistance;
        private float _firstFloatDistanceDuration;

        override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            base.OnStateEnter(animator, stateInfo, layerIndex);
            Data.BossLaserState();
            TotalAttacksPreformed++;
            Data.lightHouseAttackGameObject.SetActive(true);
            _firstFloatDistance = Data.floatingAttributes.floatDistance;
            _firstFloatDistanceDuration = Data.floatingAttributes.duration;
            Data.floatingAttributes.floatDistance = BossBrain.bossConfigurations.LightHouse.floatBossDistance;
            Data.floatingAttributes.duration = BossBrain.bossConfigurations.LightHouse.floatBossDuration;
            lightHouseAttackLogic = Data.lightHouseAttackGameObject.GetComponent<LightHouseAttackLogic>();
        }

        override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            if (Data.WaterStateActivated || lightHouseAttackLogic.HasFinishedAction)
            {
                Data.floatingAttributes.floatDistance = _firstFloatDistance;
                Data.floatingAttributes.duration = _firstFloatDistanceDuration;
                animator.SetTrigger(AttackFinished);
                Data.lightHouseAttackGameObject.SetActive(false);
            }
        }
        
    }
}