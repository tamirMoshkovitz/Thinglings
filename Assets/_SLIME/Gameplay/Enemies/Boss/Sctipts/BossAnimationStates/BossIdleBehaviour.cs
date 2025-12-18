using UnityEngine;
using DG.Tweening;

public class BossIdleBehaviour : BossBaseBehaviour
{
    private static readonly int FinishedMovement = Animator.StringToHash("FinishedMovement");

    [Header("Idle Settings")]
    public float duration = 3.0f; // How long to wait before next action
    public float floatStrength = 1f;
    
    private float _timer;

    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        base.OnStateEnter(animator, stateInfo, layerIndex);
        if (data == null) return;

        _timer = 0f;
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


}