using _SLIME.Projectiles;
using _SLIME.Slime;
using UnityEngine;

namespace _SLIME.Boss
{
    public class OneSpellShotLogic: ISpellAttackLogic
    {
        private readonly GameObject _spellPrefab;
        private readonly BossBrain _data;
        private readonly GameObject _spellSpawnPrefab;
        private SpellBeforeSpawn _spellBeforeSpawn;
        private Spell _spell;
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
        }

        public void UpdateAttack()
        {
            if (_spell == null || !_spell.HasStartedFlying()) return;
            _spell = null;
            _spellBeforeSpawn = null;
            _isActive = false;
        }

        public void Reset()
        {
            _isActive = false;
            _targetPosition = Vector3.zero;
            if (_spellBeforeSpawn != null)
            {
                Object.Destroy(_spellBeforeSpawn.gameObject);
                _spellBeforeSpawn = null;
            }
            _spell = null;
            _spellSets = null;
        }
    
        public void Attack(SpellSettings spellSets)
        {
            _isActive = true;
            _spellSets = spellSets;
            _targetPosition = GetTargetPosition();
            (_spellBeforeSpawn, _spell) = CreateSpellWithTelegraph(spellSets, _targetPosition);
        }

        public (SpellBeforeSpawn, Spell) CreateSpellWithTelegraph(SpellSettings spellSets, Vector3 targetPosition)
        {
            Vector3 spawnPos = _data.spawnDeps.spawnPoint.position;
            float accuracy = Random.Range(spellSets.attackAccuracyRange.x, spellSets.attackAccuracyRange.y);
            Vector3 direction = CalculateDirectionWithAccuracy(spawnPos, targetPosition, accuracy);
            Vector3 actualTarget = spawnPos + direction * Vector3.Distance(spawnPos, targetPosition);
            float speed = Random.Range(spellSets.attackSpeedRange.x, spellSets.attackSpeedRange.y);

            GameObject telegraphObj = Object.Instantiate(_spellSpawnPrefab, spawnPos, _spellSpawnPrefab.transform.rotation, _data.animator.transform);
            var spellBeforeSpawn = telegraphObj.GetComponent<SpellBeforeSpawn>();
            float angleToAdd = Mathf.Atan2(direction.x, -direction.y) * Mathf.Rad2Deg;
            if (angleToAdd < 0f) angleToAdd += 360f;
            float currentZ = spellBeforeSpawn.transform.eulerAngles.z;
            float totalZ = currentZ + angleToAdd;
            if (totalZ >= 360f) totalZ -= 360f;
            if (totalZ < 0f) totalZ += 360f;
            spellBeforeSpawn.transform.rotation = Quaternion.Euler(0f, 0f, totalZ);

            Vector3 spellPos = spellBeforeSpawn.comp.spawnPoint.position;
            float spellZ = Mathf.Atan2(direction.x, -direction.y) * Mathf.Rad2Deg;
            if (spellZ < 0f) spellZ += 360f;
            Quaternion spellRot = Quaternion.Euler(0f, 0f, spellZ);
            GameObject spellObj = Object.Instantiate(_spellPrefab, spellPos, spellRot);
            var spell = spellObj.GetComponentInChildren<Spell>();
            spell.BossSetup(new SpellBossAttributes
            {
                spawnPosition = spellPos,
                targetPosition = actualTarget,
                moveSpeed = speed,
                z = totalZ,
                scaleUpCurve = spellSets.scaleUpCurve,
                scaleUpDurationFactor = spellSets.scaleUpDurationFactor,
                scaleStart = spellSets.scaleStart,
                scaleUpCloseDistanceThreshold = spellSets.scaleUpCloseDistanceThreshold,
                scaleUpDurationWhenClose = spellSets.scaleUpDurationWhenClose,
                lobArcHeight = spellSets.lobArcHeight
            });
            spell.SetWaitingForSpawn(spellBeforeSpawn);
            return (spellBeforeSpawn, spell);
        }

        public SpellBeforeSpawn BeforeAttackEffect(SpellSettings spellSets, Vector3 targetPosition)
        {
            Vector3 spawnPos = _data.spawnDeps.spawnPoint.position;
            float accuracy = Random.Range(spellSets.attackAccuracyRange.x, spellSets.attackAccuracyRange.y);
            Vector3 direction = CalculateDirectionWithAccuracy(spawnPos, targetPosition, accuracy);
            GameObject telegraphObj = Object.Instantiate(_spellSpawnPrefab, spawnPos, _spellSpawnPrefab.transform.rotation, _data.animator.transform);
            var spellBeforeSpawn = telegraphObj.GetComponent<SpellBeforeSpawn>();
            float angleToAdd = Mathf.Atan2(direction.x, -direction.y) * Mathf.Rad2Deg;
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
            float accuracy = Random.Range(spellSets.attackAccuracyRange.x, spellSets.attackAccuracyRange.y);
            Vector3 direction = CalculateDirectionWithAccuracy(spawnPoint, targetPosition, accuracy);
            float speed = Random.Range(spellSets.attackSpeedRange.x, spellSets.attackSpeedRange.y);

            GameObject item = Object.Instantiate(_spellPrefab, spawnPoint, Quaternion.identity);
            Spell spell = item.GetComponentInChildren<Spell>();
            spell.BossSetup(new SpellBossAttributes
            {
                spawnPosition = spawnPoint,
                targetPosition = spawnPoint + direction * Vector3.Distance(spawnPoint, targetPosition),
                moveSpeed = speed,
                z = z,
                scaleUpCurve = spellSets.scaleUpCurve,
                scaleUpDurationFactor = spellSets.scaleUpDurationFactor,
                scaleStart = spellSets.scaleStart,
                scaleUpCloseDistanceThreshold = spellSets.scaleUpCloseDistanceThreshold,
                scaleUpDurationWhenClose = spellSets.scaleUpDurationWhenClose,
                lobArcHeight = spellSets.lobArcHeight
            });
        }
        
        
        private Vector3 GetTargetPosition()
        {
            Vector3 slime1Pos = SlimeData.instance.SideATransform.position;
            Vector3 slime2Pos = SlimeData.instance.SideBTransform.position;

            if (SlimeData.instance.SideBDead) return slime1Pos;
            if (SlimeData.instance.SideADead) return slime2Pos;
            
            Vector3 spawnPos = _data.spawnDeps.spawnPoint.position;
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