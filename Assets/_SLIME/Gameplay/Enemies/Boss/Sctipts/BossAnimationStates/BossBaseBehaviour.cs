using UnityEngine;

namespace _SLIME.Boss
{
    public class BossBaseBehaviour : StateMachineBehaviour
    {
        protected BossBrain Data;

        public void Initialize(BossBrain brain)
        {
            Data = brain;
        }

        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            if (Data == null) Data = animator.GetComponent<BossBrain>();
        }
    }
    
}
