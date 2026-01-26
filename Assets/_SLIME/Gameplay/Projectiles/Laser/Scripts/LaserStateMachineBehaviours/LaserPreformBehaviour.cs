using _SLIME.Boss;
using UnityEngine;
using UnityEngine.Serialization;

namespace _SLIME.Laser
{
    public class LaserPreformBehaviour : StateMachineBehaviour
    {
       
        private static readonly int ExitLaser = Animator.StringToHash("ExitLaser");

        override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {

            var logic = animator.GetComponent<LaserAttackLogic>();
            if (logic != null)
            {
                logic.PlayRotation(
                    BossBrain.bossConfigurations.LaserAttack.rotationCurve,
                    BossBrain.bossConfigurations.LaserAttack.rotationDuration,
                    BossBrain.bossConfigurations.LaserAttack.totalLoops
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
        
    }
}