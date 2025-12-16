using UnityEngine;
using DG.Tweening;
public class BossMoveBehaviour : BossBaseBehaviour
{
    public enum MoveType { IdleHover, Hide, Emerge }
    public MoveType moveType;
    private Tween _moveTween;

    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        base.OnStateEnter(animator, stateInfo, layerIndex);
        animator.SetTrigger("IntroFinished");
        if (data == null) return;

        data.bossRoot.DOKill();

        switch (moveType)
        {
            case MoveType.IdleHover:
                data.bossCollider.enabled = true;
                // Loop relative to the End Point (Attack Position)
                _moveTween = data.bossRoot.DOMoveY(data.endPoint.position.y + data.hoverStrength, data.hoverDuration)
                    .SetLoops(-1, LoopType.Yoyo)
                    .SetEase(Ease.InOutSine);
                break;

            case MoveType.Hide:
                data.bossCollider.enabled = false;
                // Move directly to Start Point
                _moveTween = data.bossRoot.DOMove(data.startPoint.position, data.moveSpeed)
                    .SetEase(Ease.InBack);
                break;

            case MoveType.Emerge:
                // Move directly to End Point
                _moveTween = data.bossRoot.DOMove(data.endPoint.position, data.moveSpeed)
                    .SetEase(Ease.OutBack)
                    .OnComplete(() => animator.SetTrigger("Finish"));
                break;
        }
        animator.SetTrigger("Finished");
    }

    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (moveType == MoveType.IdleHover && _moveTween != null)
        {
            _moveTween.Kill();
        }
    }
}
