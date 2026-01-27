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

        public void UpdateAttack()
        {
            if (_spellBefore1 == null || _spellBefore2 == null) return;
            if (!_spellBefore1.GetState() || !_spellBefore2.GetState()) return;

            Vector3 spawn1 = _spellBefore1.GetSpawnPoint();
            Vector3 spawn2 = _spellBefore2.GetSpawnPoint();
            float z1 = _spellBefore1.transform.eulerAngles.z;
            float z2 = _spellBefore2.transform.eulerAngles.z;

            Object.Destroy(_spellBefore1.gameObject);
            Object.Destroy(_spellBefore2.gameObject);
            _spellBefore1 = null;
            _spellBefore2 = null;

            _oneSpellShotLogic.Attack(_spellSets, _slime1Pos, spawn1, z1);
            _oneSpellShotLogic.Attack(_spellSets, _slime2Pos, spawn2, z2);
            _isActive = false;
        }

        public void Reset()
        {
            _isActive = false;
            if (_spellBefore1 != null) { Object.Destroy(_spellBefore1.gameObject); _spellBefore1 = null; }
            if (_spellBefore2 != null) { Object.Destroy(_spellBefore2.gameObject); _spellBefore2 = null; }
        }

        public void Attack(SpellSettings spellSets)
        {
            _slime1Pos = SlimeData.instance.SideATransform.position;
            _slime2Pos = SlimeData.instance.SideBTransform.position;

            _spellSets = spellSets;
            _spellSets.attackAccuracyRange = Vector2.one;
            _spellSets.attackSpeedRange = new Vector2(spellSets.attackSpeedRange.x, spellSets.attackSpeedRange.x);

            _spellBefore1 = _oneSpellShotLogic.BeforeAttackEffect(_slime1Pos);
            _spellBefore2 = _oneSpellShotLogic.BeforeAttackEffect(_slime2Pos);
            _isActive = true;
        }
    }
}
