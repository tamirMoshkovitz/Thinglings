using _SLIME.Boss;
using UnityEngine;

public class LaserPreformBehaviour : StateMachineBehaviour
{
    [SerializeField] private BaseBossSettings bossSettings;
    private static readonly int ExitLaser = Animator.StringToHash("ExitLaser");

    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {

        var logic = animator.GetComponent<LaserAttackLogic>();
        if (logic != null)
        {
            logic.PlayRotation(
                bossSettings.LaserAttack.rotationCurve,
                bossSettings.LaserAttack.rotationDuration,
                bossSettings.LaserAttack.totalLoops
            );
        }
    }

    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        var logic = animator.GetComponent<LaserAttackLogic>();
        
        if (logic != null && !logic.IsRotating)
        {
            animator.SetTrigger(ExitLaser);
        }
    }

    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        var logic = animator.GetComponent<LaserAttackLogic>();
        if (logic != null)
        {
            logic.StopRotation();
        }
    }
}