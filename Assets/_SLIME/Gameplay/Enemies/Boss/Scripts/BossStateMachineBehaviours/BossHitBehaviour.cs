using UnityEngine;

namespace _SLIME.Boss
{
    public class BossHitBehaviour: BossBaseBehaviour
    {
        override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            base.OnStateEnter(animator, stateInfo, layerIndex);
            Data.IsTakingDamage = true;
        }
        
        
        override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            base.OnStateExit(animator, stateInfo, layerIndex);
            Data.IsTakingDamage = false;
        }
    }
}