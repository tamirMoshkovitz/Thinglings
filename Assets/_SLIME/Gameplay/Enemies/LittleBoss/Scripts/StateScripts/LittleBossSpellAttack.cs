using _SLIME.Boss;

namespace _SLIME.LittleBoss
{
    public class LittleBossSpellAttack: LittleBossBaseState
    {
        private LittleBossSpellAttackLogic _logic;
        public override void Init(LittleBossBrain brain)
        {
            base.Init(brain);
            _logic = new LittleBossSpellAttackLogic(
                curSet.LittleBossSpellAttack, brain.SpellRef, brain);
            Logic = _logic;
        }

        public override void UpdateSet(BaseBossSettings newSet)
        {
            base.UpdateSet(newSet);
            _logic.Set = curSet.LittleBossSpellAttack;
        }
    }
}