using UnityEngine;
using UnityEngine.Serialization;

namespace _SLIME.Laser
{
    public class LaserPreformBehaviour : StateMachineBehaviour
    {
        [FormerlySerializedAs("bossSettings")] [SerializeField] private BaseBossConfigurations bossConfigurations;
        private static readonly int ExitLaser = Animator.StringToHash("ExitLaser");

        override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {

            var logic = animator.GetComponent<LaserAttackLogic>();
            if (logic != null)
            {
                logic.PlayRotation(
                    bossConfigurations.LaserAttack.rotationCurve,
                    bossConfigurations.LaserAttack.rotationDuration,
                    bossConfigurations.LaserAttack.totalLoops
                );
            }
        }

        override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            var logic = animator.GetComponent<LaserAttackLogic>();
        
            if (logic != null && !logic.IsRotating)
            {
                animator.SetTrigger(ExitLaser);
            }
        }

        override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            var logic = animator.GetComponent<LaserAttackLogic>();
            if (logic != null)
            {
                logic.StopRotation();
            }
        }
    }
}