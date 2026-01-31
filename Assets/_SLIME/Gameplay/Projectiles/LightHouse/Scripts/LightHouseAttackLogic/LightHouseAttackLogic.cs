using System.Collections;
using System.Collections.Generic;
using _SLIME.BaseScripts;
using _SLIME.Boss;
using _SLIME.Slime;
using UnityEngine;

namespace _SLIME.LightHouse
{
    public class LightHouseAttackLogic: ProjectMonoBehavior
    {
        [SerializeField] private LightHouseDeps lightHouseDeps;
        
        private LightHouseSets _lightHouseSets;
        public bool HasFinishedAction { get; set; }
        public bool FinishedAttack { get; set; }
        
        private struct LaserInfo
        {
            public Transform rotationPoint;
            public float distanceFromCenter;
        }
        
        private void Awake()
        {
            foreach (var c in lightHouseDeps.colliders) c.enabled = false;
            _lightHouseSets = BossBrain.bossConfigurations.LightHouse;
            InitializeLocations();
        }

        private void InitializeLocations()
        {
            Transform furthestSlime = GetFurthestSlime();
            Transform center = lightHouseDeps.lighthouseCenter;
            Vector3 centerPos = center.position;
            float furthestSlimeDist = Vector2.Distance(centerPos, furthestSlime.position);
            
            var lasers = new List<LaserInfo>
            {
                new() { rotationPoint = lightHouseDeps.closeLaserRotationPoint, distanceFromCenter = Vector2.Distance(centerPos, lightHouseDeps.closeLaserHitPoint.position) },
                new() { rotationPoint = lightHouseDeps.midLaserRotationPoint, distanceFromCenter = Vector2.Distance(centerPos, lightHouseDeps.midLaserHitPoint.position) },
                new() { rotationPoint = lightHouseDeps.farLaserRotationPoint, distanceFromCenter = Vector2.Distance(centerPos, lightHouseDeps.farLaserHitPoint.position) }
            };
            
            int mainIndex = 0;
            float bestDiff = Mathf.Abs(lasers[0].distanceFromCenter - furthestSlimeDist);
            for (int i = 1; i < lasers.Count; i++)
            {
                float diff = Mathf.Abs(lasers[i].distanceFromCenter - furthestSlimeDist);
                if (diff < bestDiff)
                {
                    bestDiff = diff;
                    mainIndex = i;
                }
            }
            
            Vector2 toSlime = (Vector2)(furthestSlime.position - centerPos);
            float atan2Deg = Mathf.Atan2(toSlime.y, toSlime.x) * Mathf.Rad2Deg;
            float slimeAngle = DirectionToLaserRotationZ(atan2Deg);
            float deviation = Random.Range(_lightHouseSets.mainBeamAngleFromSlimes.x, _lightHouseSets.mainBeamAngleFromSlimes.y);
            if (Random.value > 0.5f) deviation = -deviation;
            float mainAngle = slimeAngle + deviation;
            
            lasers[mainIndex].rotationPoint.rotation = Quaternion.Euler(0f, 0f, mainAngle);
            
            var otherIndices = new List<int>();
            for (int i = 0; i < lasers.Count; i++)
                if (i != mainIndex) otherIndices.Add(i);
            
            float minDist = _lightHouseSets.minBeamAngleDistance;
            var usedAngles = new List<float> { mainAngle };
            foreach (int idx in otherIndices)
            {
                float angle = PickAngleAwayFrom(usedAngles, mainAngle, _lightHouseSets.angleFromMainBeam.x, _lightHouseSets.angleFromMainBeam.y, minDist);
                lasers[idx].rotationPoint.rotation = Quaternion.Euler(0f, 0f, angle);
                usedAngles.Add(angle);
            }
        }
        
        private float PickAngleAwayFrom(List<float> usedAngles, float mainAngle, float minOffset, float maxOffset, float minDistance)
        {
            for (int attempt = 0; attempt < 50; attempt++)
            {
                float offset = Random.Range(minOffset, maxOffset);
                if (Random.value > 0.5f) offset = -offset;
                float candidate = mainAngle + offset;
                candidate = NormalizeAngle(candidate);
                bool ok = true;
                foreach (float used in usedAngles)
                {
                    float diff = Mathf.Abs(Mathf.DeltaAngle(candidate, used));
                    if (diff < minDistance) { ok = false; break; }
                }
                if (ok) return candidate;
            }
            return NormalizeAngle(mainAngle + 120f);
        }
        
        private static float NormalizeAngle(float a)
        {
            while (a < 0) a += 360f;
            while (a >= 360f) a -= 360f;
            return a;
        }
        
        /// <summary>
        /// Converts world direction (Atan2: 0°=right, 90°=up, 180°=left, -90°=down) to laser rotation Z.
        /// Laser: Z=0=left, Z=90=down, Z=180=right, Z=270=up.
        /// </summary>
        private static float DirectionToLaserRotationZ(float atan2Deg)
        {
            return NormalizeAngle(atan2Deg + 180f);
        }


        private void OnDisable()
        {
            HasFinishedAction = false;
            FinishedAttack = false;
            lightHouseDeps.lightHouseSfx.Stop();
        }

        public void EnableColliders() // called by animation event
        {
            foreach (var c in lightHouseDeps.colliders) c.enabled = true;
        }

        public void PlaySound() // called by animation event
        {
            lightHouseDeps.lightHouseSfx.Play();
        }
        
        
        public void Activate()
        {
            StartCoroutine(ActivateAttack());
        }

        private IEnumerator ActivateAttack()
        {
            float duration = Random.Range(_lightHouseSets.attackDuration.x, _lightHouseSets.attackDuration.y);
            float mainSpeed = Random.Range(_lightHouseSets.mainBeamSpeedPerSecond.x, _lightHouseSets.mainBeamSpeedPerSecond.y);
            float otherSpeed = Random.Range(_lightHouseSets.otherBeamsSpeedPerSecond.x, _lightHouseSets.otherBeamsSpeedPerSecond.y);
            
            var rotPoints = new[] { lightHouseDeps.closeLaserRotationPoint, lightHouseDeps.midLaserRotationPoint, lightHouseDeps.farLaserRotationPoint };
            var hitPoints = new[] { lightHouseDeps.closeLaserHitPoint, lightHouseDeps.midLaserHitPoint, lightHouseDeps.farLaserHitPoint };
            
            int mainIndex = GetMainBeamIndex(rotPoints, hitPoints);
            int mainDir = GetMainBeamDirection(rotPoints[mainIndex]);
            int otherDir = -mainDir;
            
            float checkTimer = 0f;
            
            while (duration > 0f)
            {
                float dt = Time.deltaTime;
                duration -= dt;
                checkTimer += dt;
                
                if (checkTimer >= _lightHouseSets.timeToCheckForMainBeanSwitch)
                {
                    checkTimer = 0f;
                    int newMain = GetMainBeamIndex(rotPoints, hitPoints);
                    if (newMain != mainIndex)
                    {
                        mainIndex = newMain;
                        mainDir = GetMainBeamDirection(rotPoints[mainIndex]);
                        otherDir = -mainDir;
                    }
                }
                
                for (int i = 0; i < 3; i++)
                {
                    float speed = i == mainIndex ? mainSpeed : otherSpeed;
                    int dir = i == mainIndex ? mainDir : otherDir;
                    float currentZ = rotPoints[i].eulerAngles.z;
                    rotPoints[i].rotation = Quaternion.Euler(0f, 0f, NormalizeAngle(currentZ + speed * dir * dt));
                }
                
                yield return null;
            }
            
            FinishedAttack = true;
            HasFinishedAction = true;
        }
        
        private int GetMainBeamIndex(Transform[] rotPoints, Transform[] hitPoints)
        {
            Transform furthest = GetFurthestSlime();
            Vector3 centerPos = lightHouseDeps.lighthouseCenter.position;
            float furthestDist = Vector2.Distance(centerPos, furthest.position);
            
            int best = 0;
            float bestDiff = float.MaxValue;
            for (int i = 0; i < 3; i++)
            {
                float hd = Vector2.Distance(centerPos, hitPoints[i].position);
                float diff = Mathf.Abs(hd - furthestDist);
                if (diff < bestDiff) { bestDiff = diff; best = i; }
            }
            return best;
        }
        
        private int GetMainBeamDirection(Transform mainBeam)
        {
            Transform furthest = GetFurthestSlime();
            Vector2 toSlime = (Vector2)(furthest.position - lightHouseDeps.lighthouseCenter.position);
            float targetZ = DirectionToLaserRotationZ(Mathf.Atan2(toSlime.y, toSlime.x) * Mathf.Rad2Deg);
            float currentZ = mainBeam.eulerAngles.z;
            float delta = Mathf.DeltaAngle(currentZ, targetZ);
            return Mathf.Abs(delta) < 0.1f ? 1 : (delta > 0 ? 1 : -1);
        }
        
        private Transform GetFurthestSlime()
        {
            var data = SlimeData.instance;
            if (data.SideADead) return data.SideBTransform;
            if (data.SideBDead) return data.SideATransform;
            Vector3 centerPos = lightHouseDeps.lighthouseCenter.position;
            float d1 = Vector2.Distance(centerPos, data.SideATransform.position);
            float d2 = Vector2.Distance(centerPos, data.SideBTransform.position);
            return d1 >= d2 ? data.SideATransform : data.SideBTransform;
        }


        private void OnTriggerEnter2D(Collider2D collision)
        {
            var rig = collision.attachedRigidbody;
            if (rig && rig.TryGetComponent<IHealth>(out IHealth h))
            {
                h.TakeDamage(); 
            }
        }

    }
}