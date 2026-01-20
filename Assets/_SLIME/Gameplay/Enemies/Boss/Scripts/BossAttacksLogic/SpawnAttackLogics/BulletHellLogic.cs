using _SLIME.Slime;
using UnityEngine;

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
        private SpellSettings _currentSpellSettings;
        
        private Vector3 _leftExtremeTarget;
        private Vector3 _rightExtremeTarget;
        private bool _goingRight;

        public bool IsActive => _isActive;

        public BulletHellLogic(OneSpellShotLogic oneSpellShotLogic, BossBrain data)
        {
            _oneSpellShotLogic = oneSpellShotLogic;
            _data = data;
        }

        public void Attack(SpellSettings spellSets)
        {
            var modifiedSpellSettings = spellSets;
            modifiedSpellSettings.attackAccuracyRange = Vector2.one;
            modifiedSpellSettings.attackSpeedRange = new Vector2(spellSets.attackSpeedRange.x, spellSets.attackSpeedRange.x);
            
            _currentSpellSettings = modifiedSpellSettings;
            _shotsFired = 0;
            _timer = 0f;
            _isActive = true;
            
            Vector3 spawnPos = (_data.leftSpawnPoint.position + _data.rightSpawnPoint.position) / 2f;
            
            Vector3 slime1Pos = SlimeData.instance.SideATransform.position;
            Vector3 slime2Pos = SlimeData.instance.SideBTransform.position;
            
            Vector3 leftSlimePos = slime1Pos.x < slime2Pos.x ? slime1Pos : slime2Pos;
            Vector3 rightSlimePos = slime1Pos.x < slime2Pos.x ? slime2Pos : slime1Pos;
            
            float extraAngle = _data.bossConfigurations.SpawnAttack.specialAttacksSettings.bulletHellTotalAngle;
            
            // Calculate extreme targets using CalculateOffsetTarget
            // Left extreme: left slime + extra angle to the left (positive angle rotates counter-clockwise)
            _leftExtremeTarget = CalculateOffsetTarget(spawnPos, leftSlimePos, -extraAngle);
            // Right extreme: right slime + extra angle to the right (negative angle rotates clockwise)
            _rightExtremeTarget = CalculateOffsetTarget(spawnPos, rightSlimePos, extraAngle);
            
            // Randomly decide direction
            _goingRight = Random.value > 0.5f;
            
            FireShot();
        }

        public void UpdateAttack()
        {
            if (!_isActive) return;
            
            float waitTime = _data.bossConfigurations.SpawnAttack.specialAttacksSettings.bulletHellWaitBetweenShots;
            
            _timer += Time.deltaTime;
            
            if (_timer >= waitTime)
            {
                _timer = 0f;
                FireShot();
            }
        }

        public void Reset()
        {
            _isActive = false;
            _shotsFired = 0;
            _timer = 0f;
        }

        private void FireShot()
        {
            // Interpolate between extremes based on shot number
            float t = _shotsFired / (float)(TotalShots - 1);
            
            Vector3 targetPos;
            if (_goingRight)
            {
                // Start from left, go to right
                targetPos = Vector3.Lerp(_leftExtremeTarget, _rightExtremeTarget, t);
            }
            else
            {
                // Start from right, go to left
                targetPos = Vector3.Lerp(_rightExtremeTarget, _leftExtremeTarget, t);
            }
            
            _oneSpellShotLogic.Attack(_currentSpellSettings, targetPos);
            _shotsFired++;
            
            if (_shotsFired >= TotalShots)
            {
                _isActive = false;
            }
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
