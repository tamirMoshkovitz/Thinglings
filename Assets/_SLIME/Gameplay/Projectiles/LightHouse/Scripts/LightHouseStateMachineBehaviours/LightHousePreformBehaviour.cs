using _SLIME.Boss;
using _SLIME.LightHouse;
using UnityEngine;
using UnityEngine.Serialization;

namespace _SLIME.Laser
{
    public class LightHousePreformBehaviour : StateMachineBehaviour
    {
        private LightHouseAttackLogic _logic;

        private static readonly int ExitLightHouse = Animator.StringToHash("ExitLightHouse");

        override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            _logic = animator.GetComponent<LightHouseAttackLogic>();
            _logic.Activate();
        }

        override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            if (_logic.FinishedAttack) animator.SetTrigger(ExitLightHouse);
            
        }
        
    }
}