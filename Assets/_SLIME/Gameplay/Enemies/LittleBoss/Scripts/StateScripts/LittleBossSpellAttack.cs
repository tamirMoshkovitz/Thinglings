using _SLIME.Boss;
using UnityEngine;

namespace _SLIME.LittleBoss
{
    public class LittleBossSpellAttack: LittleBossBaseState
    {
        private LittleBossSpellAttackLogic _logic;


        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            _logic = new LittleBossSpellAttackLogic(curSet.LittleBossSpellAttack, Data.SpellRef);
            Logic = _logic;
            base.OnStateEnter(animator, stateInfo, layerIndex);
        }
    }
}