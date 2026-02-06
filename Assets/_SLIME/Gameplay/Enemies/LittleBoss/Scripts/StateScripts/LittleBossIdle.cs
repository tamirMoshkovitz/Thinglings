using _SLIME.Boss;
using UnityEngine;

namespace _SLIME.LittleBoss
{
    public class LittleBossIdle : LittleBossBaseState
    {
        private LittleBossIdleLogic _logic;


        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            _logic = new LittleBossIdleLogic(curSet.LittleBossIdle, Data.IdleRef);
            Logic = _logic;
            base.OnStateEnter(animator, stateInfo, layerIndex);
        }
    }
}