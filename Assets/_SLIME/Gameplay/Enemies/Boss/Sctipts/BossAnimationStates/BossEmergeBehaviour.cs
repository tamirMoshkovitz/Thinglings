using UnityEngine;
using DG.Tweening;

public class BossEmergeBehaviour : BossBaseBehaviour
{
    private static readonly int FinishedMovement = Animator.StringToHash("FinishedMovement");

    [Header("Settings")]
    public float duration = 1.0f;

    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        base.OnStateEnter(animator, stateInfo, layerIndex);
        if (data == null) return;

        // Kill old tweens
        data.bossRoot.DOKill();

        // Move to Attack Position
        data.bossRoot.DOMove(data.endPoint.position, duration)
            .SetEase(Ease.OutBack)
            .OnComplete(() => 
            {
                // Enable collider and tell Animator we are done
                data.bossCollider.enabled = true;
                animator.SetTrigger(FinishedMovement);
            });
    }
}