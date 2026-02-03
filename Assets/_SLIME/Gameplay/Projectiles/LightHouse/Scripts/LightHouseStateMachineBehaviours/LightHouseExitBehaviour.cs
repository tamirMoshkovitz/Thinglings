using _SLIME.Laser;
using _SLIME.LightHouse;
using UnityEngine;

public class LightHouseExitBehaviour : StateMachineBehaviour
{
    private LightHouseAttackLogic _logic;
    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        base.OnStateEnter(animator, stateInfo, layerIndex);
        _logic = animator.GetComponent<LightHouseAttackLogic>();
    }
    
    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        _logic.HasFinishedAction = true;
    }
}
