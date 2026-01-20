using _SLIME.Boss;
using UnityEngine;

public class CloseToFarTransition : BossBaseBehaviour
{
    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        base.OnStateEnter(animator, stateInfo, layerIndex);
        Data.BossFarState();
    }
}
