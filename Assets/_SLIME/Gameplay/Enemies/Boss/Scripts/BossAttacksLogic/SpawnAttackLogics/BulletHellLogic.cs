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
        private SpellSettings _currentSpellSettings;
        
        private Vector3 _leftExtremeTarget;
        private Vector3 _rightExtremeTarget;
        private bool _goingRight;
        private float _densityPower;
        private SpellSpecialAttacksSettings _specials;
        public bool IsActive => _isActive;

        public BulletHellLogic(OneSpellShotLogic oneSpellShotLogic, BossBrain data)
        {
            _oneSpellShotLogic = oneSpellShotLogic;
            _data = data;
            _specials = BossBrain.bossConfigurations.SpawnAttack.specialAttacksSettings;
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
            
        
            float configuredAngle = _specials.bulletHellTotalAngle;
            float extraAngle = Mathf.Abs(configuredAngle);
            _densityPower = _specials.bulletHellMiddleDensityPower;
            
            // Base direction: straight down from spawn position
            Vector3 baseDirection = Vector3.down;
            
            var cam = Camera.main;
            
            float targetDistance = cam.orthographicSize * 2f;
            

            Quaternion leftRotation = Quaternion.Euler(0f, 0f, -extraAngle);
            Vector3 leftDirection = leftRotation * baseDirection;
            _leftExtremeTarget = spawnPos + leftDirection * targetDistance;
            
    
            Quaternion rightRotation = Quaternion.Euler(0f, 0f, extraAngle);
            Vector3 rightDirection = rightRotation * baseDirection;
            _rightExtremeTarget = spawnPos + rightDirection * targetDistance;
            
            // Determine direction based on slime positions
            // If slimes are more on the right side, start from right (going left)
            // Otherwise start from left (going right)
            Vector3 slime1Pos = SlimeData.instance.SideATransform.position;
            Vector3 slime2Pos = SlimeData.instance.SideBTransform.position;
            float avgSlimeX = (slime1Pos.x + slime2Pos.x) / 2f;
            _goingRight = avgSlimeX <= spawnPos.x; // If slimes are left of center, go right (left->right)
            
            FireShot();
        }

        public void UpdateAttack()
        {
            if (!_isActive) return;
            
            float waitTime = BossBrain.bossConfigurations.SpawnAttack.specialAttacksSettings.bulletHellWaitBetweenShots;
            
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
            
            // Apply easing function to make shots denser in the middle
            // This uses a smoothstep-like curve that concentrates more shots around 0.5
            float easedT = ApplyDenseMiddleEasing(t);
            
            Vector3 targetPos;
            if (_goingRight)
            {
                // Start from left, go to right
                targetPos = Vector3.Lerp(_leftExtremeTarget, _rightExtremeTarget, easedT);
            }
            else
            {
                // Start from right, go to left
                targetPos = Vector3.Lerp(_rightExtremeTarget, _leftExtremeTarget, easedT);
            }
            
            _oneSpellShotLogic.Attack(_currentSpellSettings, targetPos);
            _shotsFired++;
            
            if (_shotsFired >= TotalShots)
            {
                _isActive = false;
            }
        }
        
        /// <summary>
        /// Applies easing to concentrate more shots in the middle (around 0.5)
        /// Uses a curve that slows down near the center, making shots denser there
        /// </summary>
        private float ApplyDenseMiddleEasing(float t)
        {
            // Use a smoothstep-like function that concentrates values around 0.5
            // This creates a curve that's steeper at the edges and flatter in the middle
            // Formula: smoothstep with adjusted curve to push more values toward center
            
            // Normalize t to be centered around 0
            float centered = (t - 0.5f) * 2f; // -1 to 1
            
            // Apply a curve that compresses the middle region
            // Using a power function that makes the curve steeper at edges
            float sign = Mathf.Sign(centered);
            float absCentered = Mathf.Abs(centered);
            
            // Apply power curve - higher power = more compression in middle
            float curved = Mathf.Pow(absCentered, _densityPower) * sign;
            
            // Convert back to 0-1 range
            return (curved + 1f) * 0.5f;
        }
        
    }
}
