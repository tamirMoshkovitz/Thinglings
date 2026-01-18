using _SLIME.Slime;
using UnityEngine;

namespace _SLIME.Boss
{
    public class TwoShotsLogic : ISpellAttackLogic
    {
        private readonly OneSpellShotLogic _oneSpellShotLogic;

        public bool IsActive => false;

        public TwoShotsLogic(OneSpellShotLogic oneSpellShotLogic)
        {
            _oneSpellShotLogic = oneSpellShotLogic;
        }

        public void UpdateAttack() { }
        
        public void Reset() { }

        public void Attack(SpellSettings spellSets)
        {
            Vector3 slime1Pos = SlimeData.instance.SideATransform.position;
            Vector3 slime2Pos = SlimeData.instance.SideBTransform.position;
            SpellSettings newSpellSettings = spellSets;
            newSpellSettings.attackAccuracyRange = Vector2.one;
            newSpellSettings.attackSpeedRange = new Vector2(newSpellSettings.attackSpeedRange.x,
                newSpellSettings.attackSpeedRange.x);
            _oneSpellShotLogic.Attack(spellSets, slime1Pos);
            _oneSpellShotLogic.Attack(spellSets, slime2Pos);
        }
    }
}
