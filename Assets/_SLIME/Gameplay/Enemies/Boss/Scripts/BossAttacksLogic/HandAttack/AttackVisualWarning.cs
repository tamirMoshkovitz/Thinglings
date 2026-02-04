using UnityEngine;

public class AttackVisualWarning : MonoBehaviour
{
    [SerializeField] private Animator animator;
    private static readonly int PlayTrigger = Animator.StringToHash("PlayWarning");

    public bool IsAnimationPlaying { get; private set; }

    public void SetSpeed(float speed)
    {
        animator.speed = speed;
    }

    public void Play()
    {
        IsAnimationPlaying = true;
        animator.SetTrigger(PlayTrigger);
    }

    public void OnAnimationComplete()
    {
        IsAnimationPlaying = false;
    }
}