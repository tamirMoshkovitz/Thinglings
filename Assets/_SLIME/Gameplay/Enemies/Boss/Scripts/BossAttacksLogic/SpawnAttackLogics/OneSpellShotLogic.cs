using _SLIME.Projectiles;
using _SLIME.Slime;
using UnityEngine;

namespace _SLIME.Boss
{
    public class OneSpellShotLogic: ISpellAttackLogic
    {
        private readonly GameObject _spellPrefab;
        private readonly BossBrain _data;

        public bool IsActive => false;

        public OneSpellShotLogic(GameObject spellPrefab, BossBrain data)
        {
            _spellPrefab = spellPrefab;
            _data = data;
        }

        public void UpdateAttack() { }
        
        public void Reset() { }
    
        public void Attack(SpellSettings spellSets)
        {
            Vector3 targetPosition = GetTargetPosition();
            Attack(spellSets, targetPosition);
        }
        
    
        public void Attack(SpellSettings spellSets, Vector3 targetPosition)
        {
            Vector3 spawnPosition = GetSpawnPosition();
            
            float accuracy = Random.Range(spellSets.attackAccuracyRange.x, spellSets.attackAccuracyRange.y);
            Vector3 direction = CalculateDirectionWithAccuracy(spawnPosition, targetPosition, accuracy);
            
            float speed = Random.Range(spellSets.attackSpeedRange.x, spellSets.attackSpeedRange.y);
            
            GameObject item = Object.Instantiate(_spellPrefab, spawnPosition, Quaternion.identity);
            Spell spell = item.GetComponentInChildren<Spell>();
            
            spell.BossSetup(new SpellBossAttributes
            {
                direction = direction,
                moveSpeed = speed
            });
        }
        
        private Vector3 GetSpawnPosition()
        {
            Transform leftSpawnPoint = _data.leftSpawnPoint;
            Transform rightSpawnPoint = _data.rightSpawnPoint;
            
            float randomX = Random.Range(leftSpawnPoint.position.x, rightSpawnPoint.position.x);
            float fixedY = leftSpawnPoint.position.y;
            
            return new Vector3(randomX, fixedY, 0f);
        }
        
        private Vector3 GetTargetPosition()
        {
            Vector3 slime1Pos = SlimeData.instance.SideATransform.position;
            Vector3 slime2Pos = SlimeData.instance.SideBTransform.position;

            if (SlimeData.instance.SideBDead) return slime1Pos;
            if (SlimeData.instance.SideADead) return slime2Pos;
            
            Vector3 spawnPos = _data.leftSpawnPoint.position;
            float dist1 = Vector3.Distance(spawnPos, slime1Pos);
            float dist2 = Vector3.Distance(spawnPos, slime2Pos);
            
            return dist1 < dist2 ? slime1Pos : slime2Pos;
        }
        
        private Vector3 CalculateDirectionWithAccuracy(Vector3 from, Vector3 to, float accuracy)
        {
            Vector3 perfectDirection = (to - from).normalized;
            
            if (accuracy >= 1f)
                return perfectDirection;
            
            // Generate a random direction offset based on inaccuracy
            float maxAngleOffset = (1f - accuracy) * 90f; // 0 accuracy = up to 90 degree offset
            float randomAngle = Random.Range(-maxAngleOffset, maxAngleOffset);
            
            // Rotate the perfect direction by the random angle (2D rotation around Z axis)
            Quaternion rotation = Quaternion.Euler(0f, 0f, randomAngle);
            Vector3 inaccurateDirection = rotation * perfectDirection;
            
            return inaccurateDirection.normalized;
        }
    }
}