using _SLIME.Laser;
using UnityEngine;

public class LaserExitBehaviour : StateMachineBehaviour
{
    private LaserAttackLogic _logic;
    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        base.OnStateEnter(animator, stateInfo, layerIndex);
        _logic = animator.GetComponent<LaserAttackLogic>();
        _logic.StopRotation();
    }
    
    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (_logic != null)
        {
            Debug.Log("Laser Exit: Setting HasFinishedAction to true");
            _logic.HasFinishedAction = true;
        }
    }
}
