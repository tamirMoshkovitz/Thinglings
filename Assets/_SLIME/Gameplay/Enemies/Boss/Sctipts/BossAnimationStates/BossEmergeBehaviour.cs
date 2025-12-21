using UnityEngine;

namespace _SLIME.Boss
{
    public class BossEmergeBehaviour : BossBaseBehaviour
    {
        private static readonly int FinishedMovement = Animator.StringToHash("FinishedMovement");

        [Header("Settings")] public float duration = 1.0f;

        override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            base.OnStateEnter(animator, stateInfo, layerIndex);
            if (data == null) return;

            animator.SetTrigger(FinishedMovement);
        }
    }
}