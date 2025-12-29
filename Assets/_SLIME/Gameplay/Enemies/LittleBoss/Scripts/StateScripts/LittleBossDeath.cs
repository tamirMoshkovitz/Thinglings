using _SLIME.Boss;

namespace _SLIME.LittleBoss
{
    public class LittleBossDeath: LittleBossBaseState
    {
        private LittleBossDeathLogic _logic;
        public override void Init(LittleBossBrain brain)
        {
            base.Init(brain);
            _logic = new LittleBossDeathLogic();
            Logic = _logic;
        }

        public override void UpdateSet(BaseBossSettings newSet)
        {
            base.UpdateSet(newSet);
        }
    } 
}