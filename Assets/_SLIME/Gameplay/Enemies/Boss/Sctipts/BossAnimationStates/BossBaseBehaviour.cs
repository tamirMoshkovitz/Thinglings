using UnityEngine;

public class BossBaseBehaviour : StateMachineBehaviour
{
    protected BossController data; // We call it 'data' because that's all it is now

    public void Initialize(BossController controller)
    {
        data = controller;
    }

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (data == null) data = animator.GetComponent<BossController>();
    }
}