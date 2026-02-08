using UnityEngine;

public class IcicleFallBehaviour : StateMachineBehaviour
{
    private IcicleLogic _icicleLogic;
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (!_icicleLogic) _icicleLogic = animator.GetComponent<IcicleLogic>();
    }
}
