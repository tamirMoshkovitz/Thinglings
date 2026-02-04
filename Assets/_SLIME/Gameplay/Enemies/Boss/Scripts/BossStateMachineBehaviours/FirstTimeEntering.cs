using UnityEngine;

public class FirstTimeEntering : StateMachineBehaviour
{
    public static bool IsFirstTimeEntering = true;
    private static readonly int PlayBossIntro = Animator.StringToHash("PlayBossIntro");
    private static readonly int RestartBoss = Animator.StringToHash("RestartBoss");

    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (IsFirstTimeEntering)
        {
            IsFirstTimeEntering = false;
            animator.SetTrigger(PlayBossIntro);
        }
        else
        {
            animator.SetTrigger(RestartBoss);
        }
    }

    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    //override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    
    //}

    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    //override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    
    //}

    // OnStateMove is called right after Animator.OnAnimatorMove()
    //override public void OnStateMove(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    // Implement code that processes and affects root motion
    //}

    // OnStateIK is called right after Animator.OnAnimatorIK()
    //override public void OnStateIK(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    // Implement code that sets up animation IK (inverse kinematics)
    //}
}
