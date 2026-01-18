using _SLIME.Slime;
using UnityEngine;

namespace _SLIME.Boss
{
    public class FourShotsLogic : ISpellAttackLogic
    {
        private readonly OneSpellShotLogic _oneSpellShotLogic;
        private readonly BossBrain _data;
        
        private bool _isActive;
        private int _shotsFired;
        private float _timer;
        private SpellSettings _currentSpellSettings;

        public bool IsActive => _isActive;

        public FourShotsLogic(OneSpellShotLogic oneSpellShotLogic, BossBrain data)
        {
            _oneSpellShotLogic = oneSpellShotLogic;
            _data = data;
        }

        public void Attack(SpellSettings spellSets)
        {
            _currentSpellSettings = spellSets;
            _shotsFired = 0;
            _timer = 0f;
            _isActive = true;
            
            FireShot();
        }

        public void UpdateAttack()
        {
            if (!_isActive) return;
            
            float waitTime = _data.bossConfigurations.SpawnAttack.specialAttacksSettings.fourShotsWaitBetweenShots;
            
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
            Vector3 slime1Pos = SlimeData.instance.SideATransform.position;
            Vector3 slime2Pos = SlimeData.instance.SideBTransform.position;
            
            Vector3 targetPos = (_shotsFired % 2 == 0) ? slime1Pos : slime2Pos;
            
            _oneSpellShotLogic.Attack(_currentSpellSettings, targetPos);
            _shotsFired++;
            
            if (_shotsFired >= 4)
            {
                _isActive = false;
            }
        }
    }
}
