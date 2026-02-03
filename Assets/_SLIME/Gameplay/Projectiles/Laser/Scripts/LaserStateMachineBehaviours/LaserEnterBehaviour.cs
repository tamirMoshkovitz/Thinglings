using _SLIME.Boss;
using UnityEngine;

namespace _SLIME.Laser
{
    public class LaserEnterBehaviour : StateMachineBehaviour
    {
        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            var logic = animator.GetComponent<LaserAttackLogic>();
            logic.SetInitialZFromSlimeDetection(BossBrain.bossConfigurations.LaserAttack.withSlimeDetection);
            
        }
    }
}