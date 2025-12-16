using UnityEngine;
using DG.Tweening;

public class BossHideBehaviour : BossBaseBehaviour
{
    private static readonly int FinishedMovement = Animator.StringToHash("FinishedMovement");

    [Header("Hidden Settings")]
    public float moveDuration = 1.0f;       // How long it takes to fly to the hidden point
    public float stayHiddenDuration = 2.0f; // How long he waits there before finishing

    private Tween _delayTween;

    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        base.OnStateEnter(animator, stateInfo, layerIndex);
        if (data == null) return;

        data.bossRoot.DOKill();

        // Disable collider immediately
        data.bossCollider.enabled = false;

        // 1. Move to Hiding Spot
        data.bossRoot.DOMove(data.startPoint.position, moveDuration)
            .SetEase(Ease.InBack)
            .OnComplete(() => 
            {
                // 2. Wait at the hiding spot
                _delayTween = DOVirtual.DelayedCall(stayHiddenDuration, () => 
                {
                    // 3. Finish state (trigger transition to Emerge)
                    animator.SetTrigger(FinishedMovement);
                });
            });
    }

    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // Safety: If we leave this state early (e.g. boss dies while hiding), kill the timer
        if (_delayTween != null)
        {
            _delayTween.Kill();
        }
    }
}