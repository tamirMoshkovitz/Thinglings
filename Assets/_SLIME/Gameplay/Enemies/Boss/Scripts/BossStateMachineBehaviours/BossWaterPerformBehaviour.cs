using UnityEngine;

namespace _SLIME.Boss
{
    
    public class BossWaterPerformBehaviour: BossBaseBehaviour
    {
        public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            Data.WaterStateActivated = false;
            Data.floatingAttributes.floatDistance = Data.firstFloatDistance;
        }
    }
}