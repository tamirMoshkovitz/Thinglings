using _SLIME.Slime;
using UnityEngine;

namespace _SLIME.Boss
{
    public class ThreeShotsLogic : ISpellAttackLogic
    {
        private readonly OneSpellShotLogic _oneSpellShotLogic;
        private readonly BossBrain _data;

        public bool IsActive => false;

        public ThreeShotsLogic(OneSpellShotLogic oneSpellShotLogic, BossBrain data)
        {
            _oneSpellShotLogic = oneSpellShotLogic;
            _data = data;
        }

        public void UpdateAttack() { }
        
        public void Reset() { }

        public void Attack(SpellSettings spellSets)
        {
            Vector3 slime1Pos = SlimeData.instance.SideATransform.position;
            Vector3 slime2Pos = SlimeData.instance.SideBTransform.position;
            Vector3 middlePos = (slime1Pos + slime2Pos) / 2f;
            
          
            Vector3 leftSlimePos = slime1Pos.x < slime2Pos.x ? slime1Pos : slime2Pos;
            Vector3 rightSlimePos = slime1Pos.x < slime2Pos.x ? slime2Pos : slime1Pos;
            
            
            Vector3 spawnPos = (_data.leftSpawnPoint.position + _data.rightSpawnPoint.position) / 2f;
            
            float angleOffset = _data.bossConfigurations.SpawnAttack.specialAttacksSettings.threeShotsAngle;
            Vector3 leftTargetWithOffset = CalculateOffsetTarget(spawnPos, leftSlimePos, -angleOffset);
            Vector3 rightTargetWithOffset = CalculateOffsetTarget(spawnPos, rightSlimePos, angleOffset);
            
            var newSpellSet = spellSets;
            newSpellSet.attackAccuracyRange = Vector2.one;
            newSpellSet.attackSpeedRange = new Vector2(newSpellSet.attackSpeedRange.x, 
                newSpellSet.attackSpeedRange.x);
            
            _oneSpellShotLogic.Attack(newSpellSet, leftTargetWithOffset);
            _oneSpellShotLogic.Attack(newSpellSet, rightTargetWithOffset);
            _oneSpellShotLogic.Attack(newSpellSet, middlePos);
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
