using _SLIME.Boss;

namespace _SLIME.LittleBoss
{
    public class LittleBossEmerge : LittleBossBaseState
    {
        private LittleBossEmergeLogic _logic;
        public override void Init(LittleBossBrain brain)
        {
            base.Init(brain);
            _logic = new LittleBossEmergeLogic();
            Logic = _logic;
        }

        public override void UpdateSet(BaseBossConfigurations newSet)
        {
            base.UpdateSet(newSet);
        }
    }
}
