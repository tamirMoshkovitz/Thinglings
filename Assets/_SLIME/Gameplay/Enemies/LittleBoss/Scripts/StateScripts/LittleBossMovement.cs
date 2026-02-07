using _SLIME.Boss;
using _SLIME.LittleBoss;
using UnityEngine;
using UnityEngine.Animations;

namespace _SLIME.LittleBoss
{
    public class LittleBossMovement : LittleBossBaseState
    {
        private LittleBossMovementLogic _logic;


        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            _logic = new LittleBossMovementLogic(
                curSet.LittleBossMovement,
                Data.MovRef);
            Logic = _logic;
            base.OnStateEnter(animator, stateInfo, layerIndex);
        }

        public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex, AnimatorControllerPlayable controller)
        {
            base.OnStateExit(animator, stateInfo, layerIndex, controller);
            Data.EnableCollider(true);
        }
    }
}
