using _SLIME.Projectiles;
using _SLIME.Slime;
using UnityEngine;

namespace _SLIME.Boss
{
    public class FourShotsLogic : ISpellAttackLogic
    {
        private const int TotalShots = 4;

        private readonly OneSpellShotLogic _oneSpellShotLogic;
        private readonly BossBrain _data;
        private readonly SpellSpecialAttacksSettings _specialSettings;

        private bool _isActive;
        private int _shotsFired;
        private float _timer;
        private SpellSettings _spellSets;

        private SpellBeforeSpawn[] _spellBefores = new SpellBeforeSpawn[TotalShots];
        private Vector3[] _targets = new Vector3[TotalShots];

        public bool IsActive => _isActive;

        public FourShotsLogic(OneSpellShotLogic oneSpellShotLogic, BossBrain data)
        {
            _oneSpellShotLogic = oneSpellShotLogic;
            _data = data;
            _specialSettings = default;
        }

        public FourShotsLogic(OneSpellShotLogic oneSpellShotLogic, SpellSpecialAttacksSettings specialSettings)
        {
            _oneSpellShotLogic = oneSpellShotLogic;
            _data = null;
            _specialSettings = specialSettings;
        }

        public void Attack(SpellSettings spellSets)
        {
            _spellSets = spellSets;
            _spellSets.attackAccuracyRange = Vector2.one;
            _spellSets.attackSpeedRange = new Vector2(spellSets.attackSpeedRange.x, spellSets.attackSpeedRange.x);

            _shotsFired = 0;
            _timer = 0f;
            _isActive = true;

            Vector3 slime1Pos = SlimeData.instance.SideATransform.position;
            Vector3 slime2Pos = SlimeData.instance.SideBTransform.position;

            for (int i = 0; i < TotalShots; i++)
            {
                _targets[i] = (i % 2 == 0) ? slime1Pos : slime2Pos;
                _spellBefores[i] = _oneSpellShotLogic.BeforeAttackEffect(_targets[i]);
            }
        }

        public void UpdateAttack()
        {
            if (!_isActive) return;
            if (_shotsFired >= TotalShots)
            {
                _isActive = false;
                return;
            }

            float waitTime = _data != null ? BossBrain.bossConfigurations.SpawnAttack.specialAttacksSettings.fourShotsWaitBetweenShots : _specialSettings.fourShotsWaitBetweenShots;
            _timer += Time.deltaTime;
            if (_timer < waitTime) return;

            int idx = _shotsFired;
            SpellBeforeSpawn before = _spellBefores[idx];
            if (before == null) return;
            if (!before.GetState()) return;

            Vector3 spawn = before.GetSpawnPoint();
            float z = before.transform.eulerAngles.z;
            Object.Destroy(before.gameObject);
            _spellBefores[idx] = null;

            _oneSpellShotLogic.Attack(_spellSets, _targets[idx], spawn, z);
            _shotsFired++;
            _timer = 0f;

            if (_shotsFired >= TotalShots)
                _isActive = false;
        }

        public void Reset()
        {
            _isActive = false;
            _shotsFired = 0;
            _timer = 0f;
            for (int i = 0; i < TotalShots; i++)
            {
                if (_spellBefores[i] != null)
                {
                    Object.Destroy(_spellBefores[i].gameObject);
                    _spellBefores[i] = null;
                }
            }
        }
    }
}
