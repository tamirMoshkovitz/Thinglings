using UnityEngine;
using DG.Tweening;

public class BossHideBehaviour : BossBaseBehaviour
{
    private static readonly int FinishedMovement = Animator.StringToHash("FinishedMovement");

    [Header("Hidden Settings")]
    public float moveDuration = 1.0f;
    public float stayHiddenDuration = 2.0f;


    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        base.OnStateEnter(animator, stateInfo, layerIndex);
        if (data == null) return;

        animator.SetTrigger(FinishedMovement);
    }
    
}