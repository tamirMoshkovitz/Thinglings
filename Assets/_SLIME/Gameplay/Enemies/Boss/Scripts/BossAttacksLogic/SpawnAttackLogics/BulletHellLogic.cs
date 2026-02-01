using _SLIME.Projectiles;
using UnityEngine;
using _SLIME.Slime;

namespace _SLIME.Boss
{
    public class BulletHellLogic : ISpellAttackLogic
    {
        private const int TotalShots = 8;

        private readonly OneSpellShotLogic _oneSpellShotLogic;
        private readonly BossBrain _data;

        private bool _isActive;
        private int _shotsFired;
        private float _timer;
        private SpellSettings _spellSets;
        private SpellSpecialAttacksSettings _specials;
        private float _densityPower;
        private Vector3 _leftExtremeTarget;
        private Vector3 _rightExtremeTarget;
        private bool _goingRight;

        private SpellBeforeSpawn[] _spellBefores = new SpellBeforeSpawn[TotalShots];
        private Vector3[] _targets = new Vector3[TotalShots];

        public bool IsActive => _isActive;

        public BulletHellLogic(OneSpellShotLogic oneSpellShotLogic, BossBrain data)
        {
            _oneSpellShotLogic = oneSpellShotLogic;
            _data = data;
            _specials = BossBrain.bossConfigurations.SpawnAttack.specialAttacksSettings;
        }

        public void Attack(SpellSettings spellSets)
        {
            _spellSets = spellSets;
            _spellSets.attackAccuracyRange = Vector2.one;
            _spellSets.attackSpeedRange = new Vector2(spellSets.attackSpeedRange.x, spellSets.attackSpeedRange.x);

            _shotsFired = 0;
            _timer = 0f;
            _isActive = true;

            Vector3 spawnPos = _data.spawnDeps.spawnPoint.position;
            float configuredAngle = _specials.bulletHellTotalAngle;
            float extraAngle = Mathf.Abs(configuredAngle);
            _densityPower = _specials.bulletHellMiddleDensityPower;

            Vector3 baseDirection = Vector3.down;
            float targetDistance = Camera.main != null ? Camera.main.orthographicSize * 2f : 12f;

            Quaternion leftRotation = Quaternion.Euler(0f, 0f, -extraAngle);
            _leftExtremeTarget = spawnPos + leftRotation * baseDirection * targetDistance;
            Quaternion rightRotation = Quaternion.Euler(0f, 0f, extraAngle);
            _rightExtremeTarget = spawnPos + rightRotation * baseDirection * targetDistance;

            Vector3 slime1Pos = SlimeData.instance.SideATransform.position;
            Vector3 slime2Pos = SlimeData.instance.SideBTransform.position;
            float avgSlimeX = (slime1Pos.x + slime2Pos.x) / 2f;
            _goingRight = avgSlimeX <= spawnPos.x;

            for (int i = 0; i < TotalShots; i++)
            {
                float t = i / (float)(TotalShots - 1);
                float easedT = ApplyDenseMiddleEasing(t);
                Vector3 targetPos = _goingRight
                    ? Vector3.Lerp(_leftExtremeTarget, _rightExtremeTarget, easedT)
                    : Vector3.Lerp(_rightExtremeTarget, _leftExtremeTarget, easedT);
                _targets[i] = targetPos;
                _spellBefores[i] = _oneSpellShotLogic.BeforeAttackEffect(targetPos);
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

            float waitTime = _specials.bulletHellWaitBetweenShots;
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

        private float ApplyDenseMiddleEasing(float t)
        {
            float centered = (t - 0.5f) * 2f;
            float sign = Mathf.Sign(centered);
            float absCentered = Mathf.Abs(centered);
            float curved = Mathf.Pow(absCentered, _densityPower) * sign;
            return (curved + 1f) * 0.5f;
        }
    }
}
