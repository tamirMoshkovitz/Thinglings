using UnityEngine;
using DG.Tweening;

public class BossHideBehaviour : BossBaseBehaviour
{
    private static readonly int FinishedMovement = Animator.StringToHash("FinishedMovement");

    [Header("Hidden Settings")]
    public float moveDuration = 1.0f;
    public float stayHiddenDuration = 2.0f;

    private Tween _delayTween;

    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        base.OnStateEnter(animator, stateInfo, layerIndex);
        if (data == null) return;

        data.bossRoot.DOKill();
        
        data.bossCollider.enabled = false;
        
        data.bossRoot.DOMove(data.startPoint.position, moveDuration)
            .SetEase(Ease.InBack)
            .OnComplete(() => 
            {
                _delayTween = DOVirtual.DelayedCall(stayHiddenDuration, () => 
                {
                    animator.SetTrigger(FinishedMovement);
                });
            });
    }

    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (_delayTween != null)
        {
            _delayTween.Kill();
        }
    }
}