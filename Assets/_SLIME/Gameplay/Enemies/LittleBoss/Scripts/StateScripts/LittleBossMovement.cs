using _SLIME.Boss;
using _SLIME.LittleBoss;
using UnityEngine;

namespace _SLIME.LittleBoss
{
    public class LittleBossMovement : LittleBossBaseState
    {
        private LittleBossMovementLogic _logic;
        public override void Init(LittleBossBrain brain)
        {
            base.Init(brain);
            _logic = new LittleBossMovementLogic(
                curSet.LittleBossMovement,
                brain.MovRef,
                brain);
            Logic = _logic;
        }

        public override void UpdateSet(BaseBossSettings newSet)
        {
            base.UpdateSet(newSet);
            _logic.Set = curSet.LittleBossMovement;
        }
    }
}
