using UnityEngine;

namespace _SLIME.Boss
{
    public class StopSpawningIcicleBehaviour: BossBaseBehaviour
    {
        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            base.OnStateEnter(animator, stateInfo, layerIndex);
            Data.spawner.CancelSpawning();
        }
    }
}