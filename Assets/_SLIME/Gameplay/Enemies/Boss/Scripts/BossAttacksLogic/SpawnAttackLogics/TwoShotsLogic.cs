using _SLIME.Projectiles;
using _SLIME.Slime;
using UnityEngine;

namespace _SLIME.Boss
{
    public class TwoShotsLogic : ISpellAttackLogic
    {
        private readonly OneSpellShotLogic _oneSpellShotLogic;

        private SpellBeforeSpawn _spellBefore1;
        private SpellBeforeSpawn _spellBefore2;
        private SpellSettings _spellSets;
        private Vector3 _slime1Pos;
        private Vector3 _slime2Pos;
        private bool _isActive;

        public bool IsActive => _isActive;

        public TwoShotsLogic(OneSpellShotLogic oneSpellShotLogic)
        {
            _oneSpellShotLogic = oneSpellShotLogic;
        }

        private Spell _spell1;
        private Spell _spell2;

        public void UpdateAttack()
        {
            if (_spell1 == null || _spell2 == null) return;
            if (!_spell1.HasStartedFlying() || !_spell2.HasStartedFlying()) return;
            _spell1 = null;
            _spell2 = null;
            _spellBefore1 = null;
            _spellBefore2 = null;
            _isActive = false;
        }

        public void Reset()
        {
            _isActive = false;
            if (_spellBefore1 != null) { Object.Destroy(_spellBefore1.gameObject); _spellBefore1 = null; }
            if (_spellBefore2 != null) { Object.Destroy(_spellBefore2.gameObject); _spellBefore2 = null; }
            _spell1 = null;
            _spell2 = null;
        }

        public void Attack(SpellSettings spellSets)
        {
            _slime1Pos = SlimeData.instance.SideATransform.position;
            _slime2Pos = SlimeData.instance.SideBTransform.position;

            _spellSets = spellSets;
            _spellSets.attackAccuracyRange = Vector2.one;
            _spellSets.attackSpeedRange = new Vector2(spellSets.attackSpeedRange.x, spellSets.attackSpeedRange.x);

            (_spellBefore1, _spell1) = _oneSpellShotLogic.CreateSpellWithTelegraph(_spellSets, _slime1Pos);
            (_spellBefore2, _spell2) = _oneSpellShotLogic.CreateSpellWithTelegraph(_spellSets, _slime2Pos);
            _isActive = true;
        }
    }
}
