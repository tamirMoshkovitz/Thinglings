using _SLIME.Projectiles;
using _SLIME.Slime;
using UnityEngine;

namespace _SLIME.Boss
{
    public class ThreeShotsLogic : ISpellAttackLogic
    {
        private readonly OneSpellShotLogic _oneSpellShotLogic;
        private readonly BossBrain _data;

        private SpellBeforeSpawn _spellBefore1;
        private SpellBeforeSpawn _spellBefore2;
        private SpellBeforeSpawn _spellBefore3;
        private Spell _spell1;
        private Spell _spell2;
        private Spell _spell3;
        private SpellSettings _spellSets;
        private Vector3 _target1;
        private Vector3 _target2;
        private Vector3 _target3;
        private bool _isActive;

        public bool IsActive => _isActive;

        public ThreeShotsLogic(OneSpellShotLogic oneSpellShotLogic, BossBrain data)
        {
            _oneSpellShotLogic = oneSpellShotLogic;
            _data = data;
        }

        public void UpdateAttack()
        {
            if (_spell1 == null || _spell2 == null || _spell3 == null) return;
            if (!_spell1.HasStartedFlying() || !_spell2.HasStartedFlying() || !_spell3.HasStartedFlying()) return;
            _spell1 = null;
            _spell2 = null;
            _spell3 = null;
            _spellBefore1 = null;
            _spellBefore2 = null;
            _spellBefore3 = null;
            _isActive = false;
        }

        public void Reset()
        {
            _isActive = false;
            if (_spellBefore1 != null) { Object.Destroy(_spellBefore1.gameObject); _spellBefore1 = null; }
            if (_spellBefore2 != null) { Object.Destroy(_spellBefore2.gameObject); _spellBefore2 = null; }
            if (_spellBefore3 != null) { Object.Destroy(_spellBefore3.gameObject); _spellBefore3 = null; }
            _spell1 = null;
            _spell2 = null;
            _spell3 = null;
        }

        public void Attack(SpellSettings spellSets)
        {
            Vector3 slime1Pos = SlimeData.instance.SideATransform.position;
            Vector3 slime2Pos = SlimeData.instance.SideBTransform.position;
            Vector3 middlePos = (slime1Pos + slime2Pos) / 2f;

            Vector3 leftSlimePos = slime1Pos.x < slime2Pos.x ? slime1Pos : slime2Pos;
            Vector3 rightSlimePos = slime1Pos.x < slime2Pos.x ? slime2Pos : slime1Pos;

            Vector3 spawnPos = _data.spawnDeps.spawnPoint.position;
            float angleOffset = BossBrain.bossConfigurations.SpawnAttack.specialAttacksSettings.threeShotsAngle;
            _target1 = CalculateOffsetTarget(spawnPos, leftSlimePos, -angleOffset);
            _target2 = CalculateOffsetTarget(spawnPos, rightSlimePos, angleOffset);
            _target3 = middlePos;

            _spellSets = spellSets;
            _spellSets.attackAccuracyRange = Vector2.one;
            _spellSets.attackSpeedRange = new Vector2(spellSets.attackSpeedRange.x, spellSets.attackSpeedRange.x);

            (_spellBefore1, _spell1) = _oneSpellShotLogic.CreateSpellWithTelegraph(_spellSets, _target1);
            (_spellBefore2, _spell2) = _oneSpellShotLogic.CreateSpellWithTelegraph(_spellSets, _target2);
            (_spellBefore3, _spell3) = _oneSpellShotLogic.CreateSpellWithTelegraph(_spellSets, _target3);
            _isActive = true;
        }

        private Vector3 CalculateOffsetTarget(Vector3 from, Vector3 to, float angleDegrees)
        {
            Vector3 direction = (to - from).normalized;
            float distance = Vector3.Distance(from, to);
            Quaternion rotation = Quaternion.Euler(0f, 0f, angleDegrees);
            Vector3 rotatedDirection = rotation * direction;
            return from + rotatedDirection * distance;
        }
    }
}
