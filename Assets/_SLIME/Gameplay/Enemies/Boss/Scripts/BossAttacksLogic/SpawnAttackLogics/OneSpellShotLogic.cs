using _SLIME.Projectiles;
using _SLIME.Slime;
using UnityEngine;

namespace _SLIME.Boss
{
    public class OneSpellShotLogic : ISpellAttackLogic
    {
        private readonly GameObject _spellPrefab;
        private readonly BossBrain _data;
        private readonly GameObject _spellSpawnPrefab;
        private readonly Transform _spawnPoint;
        private readonly Transform _parent;
        private SpellBeforeSpawn _spellBeforeSpawn;
        private Vector3 _targetPosition;
        private SpellSettings? _spellSets;
        private bool _isActive;
        public bool IsActive => _isActive;

        public OneSpellShotLogic(GameObject spellPrefab,
            GameObject spellSpawnPrefab, BossBrain data)
        {
            _spellPrefab = spellPrefab;
            _spellSpawnPrefab = spellSpawnPrefab;
            _data = data;
            _spawnPoint = null;
            _parent = null;
        }

        public OneSpellShotLogic(GameObject spellPrefab, GameObject spellSpawnPrefab,
            Transform spawnPoint, Transform parent)
        {
            _spellPrefab = spellPrefab;
            _spellSpawnPrefab = spellSpawnPrefab;
            _data = null;
            _spawnPoint = spawnPoint;
            _parent = parent;
        }

        public void UpdateAttack()
        {
            if (!_spellBeforeSpawn.GetState()) return;
            Vector3 spawnPos = _spellBeforeSpawn.GetSpawnPoint();
            var z = _spellBeforeSpawn.transform.eulerAngles.z;
            Object.Destroy(_spellBeforeSpawn.gameObject);
            _spellBeforeSpawn = null;
            Attack(_spellSets.Value, _targetPosition,spawnPos, z);
            _isActive = false;
        }

        public void Reset()
        {
            _isActive = false;
            _targetPosition = Vector3.zero;
            if(_spellBeforeSpawn) Object.Destroy(_spellBeforeSpawn.gameObject);
            _spellBeforeSpawn = null;
            _spellSets = null;
        }
    
        public void Attack(SpellSettings spellSets)
        {
            _isActive = true;
            _spellSets = spellSets;
            Vector3 spawnPos = _data != null ? _data.SpawnPoint.position : _spawnPoint.position;
            bool isMiddle;
            Vector3 perfectTarget = GetPerfectTarget(spellSets.targetMiddleProbability, out isMiddle);
            _targetPosition = isMiddle
                ? perfectTarget
                : GetTargetWithAccuracy(spawnPos, perfectTarget, Random.Range(spellSets.attackAccuracyRange.x, spellSets.attackAccuracyRange.y));
            _spellBeforeSpawn = BeforeAttackEffect(_targetPosition);
        }

        private Vector3 GetPerfectTarget(float targetMiddleProbability, out bool targetedMiddle)
        {
            targetedMiddle = false;
            if (SlimeData.instance.SideADead || SlimeData.instance.SideBDead)
                return GetTargetPosition();
            if (targetMiddleProbability > 0f && Random.value < targetMiddleProbability)
            {
                targetedMiddle = true;
                Vector3 s1 = SlimeData.instance.SideATransform.position;
                Vector3 s2 = SlimeData.instance.SideBTransform.position;
                return (s1 + s2) * 0.5f;
            }
            return GetTargetPosition();
        }

        public SpellBeforeSpawn BeforeAttackEffect(Vector3 targetPosition)
        {
            Vector3 spawnPos = _data != null ? _data.SpawnPoint.position : _spawnPoint.position;
            Transform parent = _data != null ? _data.animator.transform : _parent;
            GameObject item = Object.Instantiate(_spellSpawnPrefab, spawnPos, _spellSpawnPrefab.transform.rotation, parent);
            var spellBeforeSpawn = item.GetComponent<SpellBeforeSpawn>();
            
            Vector2 dir = (targetPosition - spawnPos).normalized;
            float angleToAdd = Mathf.Atan2(dir.x, -dir.y) * Mathf.Rad2Deg;
            if (angleToAdd < 0f) angleToAdd += 360f;
            float currentZ = spellBeforeSpawn.transform.eulerAngles.z;
            float totalZ = currentZ + angleToAdd;
            if (totalZ >= 360f) totalZ -= 360f;
            if (totalZ < 0f) totalZ += 360f;
            spellBeforeSpawn.transform.rotation = Quaternion.Euler(0f, 0f, totalZ);
            return spellBeforeSpawn;
        }


        public void Attack(SpellSettings spellSets, Vector3 targetPosition, Vector3 spawnPoint, float z)
        {
            Vector3 direction = (targetPosition - spawnPoint).normalized;
            
            float speed = Random.Range(spellSets.attackSpeedRange.x, spellSets.attackSpeedRange.y);
            
            GameObject item = Object.Instantiate(_spellPrefab, spawnPoint, Quaternion.identity);
            Spell spell = item.GetComponentInChildren<Spell>();
            
            spell.BossSetup(new SpellBossAttributes
            {
                direction = direction,
                moveSpeed = speed,
                z = z
            });
        }
        
        
        private Vector3 GetTargetPosition()
        {
            Vector3 slime1Pos = SlimeData.instance.SideATransform.position;
            Vector3 slime2Pos = SlimeData.instance.SideBTransform.position;

            if (SlimeData.instance.SideBDead) return slime1Pos;
            if (SlimeData.instance.SideADead) return slime2Pos;

            Vector3 spawnPos = _data != null ? _data.SpawnPoint.position : _spawnPoint.position;
            float dist1 = Vector3.Distance(spawnPos, slime1Pos);
            float dist2 = Vector3.Distance(spawnPos, slime2Pos);
            
            return dist1 < dist2 ? slime1Pos : slime2Pos;
        }
        
        private Vector3 GetTargetWithAccuracy(Vector3 from, Vector3 perfectTarget, float accuracy)
        {
            Vector3 direction = CalculateDirectionWithAccuracy(from, perfectTarget, accuracy);
            float distance = Vector3.Distance(from, perfectTarget);
            return from + direction * distance;
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