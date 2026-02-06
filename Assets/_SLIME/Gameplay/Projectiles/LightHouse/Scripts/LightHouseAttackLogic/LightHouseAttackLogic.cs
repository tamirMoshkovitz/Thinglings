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
        private float _mainDirectionEffective;
        private int _mainBeamIndex;
        private bool _transitionInProgress;
        
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
            if (furthestSlime == null) return;

            UpdateMainBeamIndex();
            Transform center = lightHouseDeps.lighthouseCenter;
            Vector3 centerPos = center.position;
            var lasers = new List<LaserInfo>
            {
                new() { rotationPoint = lightHouseDeps.closeLaserRotationPoint, distanceFromCenter = Vector2.Distance(centerPos, lightHouseDeps.closeLaserHitPoint.position) },
                new() { rotationPoint = lightHouseDeps.midLaserRotationPoint, distanceFromCenter = Vector2.Distance(centerPos, lightHouseDeps.midLaserHitPoint.position) },
                new() { rotationPoint = lightHouseDeps.farLaserRotationPoint, distanceFromCenter = Vector2.Distance(centerPos, lightHouseDeps.farLaserHitPoint.position) }
            };

            Vector2 toSlime = (Vector2)(furthestSlime.position - centerPos);
            float atan2Deg = Mathf.Atan2(toSlime.y, toSlime.x) * Mathf.Rad2Deg;
            float slimeAngle = DirectionToLaserRotationZ(atan2Deg);
            float deviation = Random.Range(_lightHouseSets.mainBeamAngleFromSlimes.x, _lightHouseSets.mainBeamAngleFromSlimes.y);
            if (Random.value > 0.5f) deviation = -deviation;
            float mainAngle = slimeAngle + deviation;
            
            lasers[_mainBeamIndex].rotationPoint.rotation = Quaternion.Euler(0f, 0f, mainAngle);
            
            var otherIndices = new List<int>();
            for (int i = 0; i < lasers.Count; i++)
                if (i != _mainBeamIndex) otherIndices.Add(i);
            
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
        
        public void DisableColliders() // called by animation event
        {
            foreach (var c in lightHouseDeps.colliders) c.enabled = false;
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
            float[] beamSpeeds = { _lightHouseSets.closeBeamSpeed, _lightHouseSets.midBeamSpeed, _lightHouseSets.farBeamSpeed };
            float transitionDuration = Mathf.Max(0.01f, _lightHouseSets.directionFlipTransitionDuration);
            float checkInterval = Mathf.Max(0.1f, _lightHouseSets.timeToCheckForMainBeanSwitch);

            UpdateMainBeamIndex();
            int desiredDir = GetDesiredMainDirection();
            _mainDirectionEffective = desiredDir;
            
            var rotPoints = new[] { lightHouseDeps.closeLaserRotationPoint, lightHouseDeps.midLaserRotationPoint, lightHouseDeps.farLaserRotationPoint };
            float timeSinceLastCheck = 0f;
            
            while (duration > 0f)
            {
                float dt = Time.deltaTime;
                duration -= dt;
                timeSinceLastCheck += dt;
                
                if (!_transitionInProgress && timeSinceLastCheck >= checkInterval)
                {
                    timeSinceLastCheck = 0f;
                    UpdateMainBeamIndex();
                    int newDesired = GetDesiredMainDirection();
                    int currentDir = _mainDirectionEffective >= 0 ? 1 : -1;
                    if (newDesired != currentDir && duration > transitionDuration)
                    {
                        _transitionInProgress = true;
                        StartCoroutine(TransitionDirection(newDesired, transitionDuration));
                        timeSinceLastCheck = -transitionDuration; // Next check only after transition + checkInterval
                    }
                }
                
                // Main beam dictates direction. Middle (index 1) always opposite to close (0) and far (2).
                for (int i = 0; i < 3; i++)
                {
                    float beamSpeed = beamSpeeds[i];
                    bool isMiddle = (i == 1);
                    float effective = isMiddle ? -_mainDirectionEffective : _mainDirectionEffective;
                    float currentZ = rotPoints[i].eulerAngles.z;
                    rotPoints[i].rotation = Quaternion.Euler(0f, 0f, NormalizeAngle(currentZ + beamSpeed * effective * dt));
                }
                
                yield return null;
            }
            
            FinishedAttack = true;
        }
        
        private IEnumerator TransitionDirection(int mainTarget, float transitionDuration)
        {
            float oldDir = _mainDirectionEffective;
            float t = 0f;
            while (t < 1f)
            {
                t += Time.deltaTime / transitionDuration;
                t = Mathf.Clamp01(t);
                float param = (1f - Mathf.Cos(Mathf.PI * t)) * 0.5f;
                _mainDirectionEffective = Mathf.Lerp(oldDir, mainTarget, param);
                yield return null;
            }
            _mainDirectionEffective = mainTarget;
            _transitionInProgress = false;
        }
        
        /// <summary>
        /// Returns 1 or -1 for main beam rotation. Laser "looks away" from center,
        /// so we choose the direction that minimizes |beamAngle - slimeAngle|.
        /// Uses dead zone to prevent oscillation when close to target.
        /// </summary>
        private int GetDesiredMainDirection()
        {
            Transform furthest = GetFurthestSlime();
            if (furthest == null) return _mainDirectionEffective >= 0 ? 1 : -1;
            Transform mainRotation = GetMainBeamRotationPoint();
            Vector3 centerPos = lightHouseDeps.lighthouseCenter.position;
            Vector2 toSlime = (Vector2)(furthest.position - centerPos);
            float atan2Deg = Mathf.Atan2(toSlime.y, toSlime.x) * Mathf.Rad2Deg;
            float slimeAngle = DirectionToLaserRotationZ(atan2Deg);
            float beamAngle = NormalizeAngle(mainRotation.eulerAngles.z);
            float delta = Mathf.DeltaAngle(beamAngle, slimeAngle);
            float deadZone = Mathf.Max(1f, _lightHouseSets.directionFlipDeadZone);
            if (Mathf.Abs(delta) < deadZone) return _mainDirectionEffective >= 0 ? 1 : -1;
            return delta > 0 ? 1 : -1;
        }
        
        private void UpdateMainBeamIndex()
        {
            Transform furthest = GetFurthestSlime();
            if (furthest == null) return;
            Transform center = lightHouseDeps.lighthouseCenter;
            Vector3 centerPos = center.position;
            float furthestSlimeDist = Vector2.Distance(centerPos, furthest.position);
            var lasers = new List<LaserInfo>
            {
                new() { rotationPoint = lightHouseDeps.closeLaserRotationPoint, distanceFromCenter = Vector2.Distance(centerPos, lightHouseDeps.closeLaserHitPoint.position) },
                new() { rotationPoint = lightHouseDeps.midLaserRotationPoint, distanceFromCenter = Vector2.Distance(centerPos, lightHouseDeps.midLaserHitPoint.position) },
                new() { rotationPoint = lightHouseDeps.farLaserRotationPoint, distanceFromCenter = Vector2.Distance(centerPos, lightHouseDeps.farLaserHitPoint.position) }
            };
            _mainBeamIndex = 0;
            float bestDiff = Mathf.Abs(lasers[0].distanceFromCenter - furthestSlimeDist);
            for (int i = 1; i < lasers.Count; i++)
            {
                float diff = Mathf.Abs(lasers[i].distanceFromCenter - furthestSlimeDist);
                if (diff < bestDiff)
                {
                    bestDiff = diff;
                    _mainBeamIndex = i;
                }
            }
        }

        private Transform GetMainBeamRotationPoint()
        {
            return _mainBeamIndex switch
            {
                0 => lightHouseDeps.closeLaserRotationPoint,
                1 => lightHouseDeps.midLaserRotationPoint,
                2 => lightHouseDeps.farLaserRotationPoint,
                _ => lightHouseDeps.midLaserRotationPoint
            };
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