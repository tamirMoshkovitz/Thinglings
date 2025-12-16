using UnityEngine;

public class BossDecideBehaviour : BossBaseBehaviour
{
    [Header("Decision Settings")]
    [Range(0, 1)] public float smashProbability = 0.5f;

    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        base.OnStateEnter(animator, stateInfo, layerIndex);

        float rng = Random.value;

        if (rng < smashProbability)
        {
            animator.SetTrigger("DoSmash");
        }
        else
        {
            animator.SetTrigger("DoSpawn");
        }
    }
}
