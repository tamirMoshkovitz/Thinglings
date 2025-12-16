using UnityEngine;
using DG.Tweening;

public class BossIdleBehaviour : BossBaseBehaviour
{
    private static readonly int FinishedMovement = Animator.StringToHash("FinishedMovement");

    [Header("Idle Settings")]
    public float duration = 3.0f; // How long to wait before next action
    public float floatStrength = 1f;
    
    private Tween _hoverTween;
    private float _timer;

    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        base.OnStateEnter(animator, stateInfo, layerIndex);
        if (data == null) return;

        _timer = 0f;
        data.bossCollider.enabled = true;
        data.bossRoot.DOKill();

        // Start infinite floating loop relative to the End Point
        _hoverTween = data.bossRoot.DOMoveY(data.endPoint.position.y + floatStrength, duration)
            .SetLoops(-1, LoopType.Yoyo)
            .SetEase(Ease.InOutSine);
    }

    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // Run the timer
        _timer += Time.deltaTime;

        if (_timer >= duration)
        {
            animator.SetTrigger(FinishedMovement);
        }
    }

    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // Stop the hovering when we leave this state
        _hoverTween?.Kill();
    }
}