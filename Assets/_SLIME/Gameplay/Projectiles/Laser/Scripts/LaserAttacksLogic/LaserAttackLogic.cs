using System;
using System.Collections;
using System.Collections.Generic;
using _SLIME.BaseScripts;
using _SLIME.Slime;
using UnityEngine;

namespace _SLIME.Laser
{
    public class LaserAttackLogic: ProjectMonoBehavior
    {
        private Coroutine _rotationCoroutine;
    
        [SerializeField] List<GameObject> laserCollidersGameObjects;
        [SerializeField] Collider2D rightLaserCollider;
        [SerializeField] Collider2D leftLaserCollider;
        [SerializeField] private ControlledSfx laserSfx;
        public bool IsRotating { get; private set; }
        
        public bool HasFinishedAction { get; set;}

        private void Awake()
        {
            rightLaserCollider.enabled = false;
            leftLaserCollider.enabled = false;
        }

        private void OnDisable()
        {
            HasFinishedAction = false;
            laserSfx.Stop();
        }
        
        public void EnableRightLaserColliders() // called by animation event
        {
            rightLaserCollider.enabled = true;
            laserSfx.Play();
        }
            
        public void EnableLeftLaserColliders() // called by animation event
        {
            leftLaserCollider.enabled = true;
        }

        /// <summary>Aligns laser Z rotation toward the midpoint between the two slimes. Call during LaserEnter when withSlimeDetection is true.</summary>
        public void SetInitialZFromSlimeDetection(bool useSlimeDetection)
        {
            if (!useSlimeDetection || SlimeData.instance == null) return;
            if (SlimeData.instance.SideADead && SlimeData.instance.SideBDead) return;

            Vector3 a = SlimeData.instance.SideADead ? SlimeData.instance.SideBTransform.position : SlimeData.instance.SideATransform.position;
            Vector3 b = SlimeData.instance.SideBDead ? SlimeData.instance.SideATransform.position : SlimeData.instance.SideBTransform.position;
            Vector3 midpoint = (a + b) * 0.5f;
            Vector2 dir = (Vector2)(midpoint - transform.position);
            if (dir.sqrMagnitude < 0.0001f) return;

            // atan2: right=0°, up=90°, left=±180°, down=-90°
            // Z mapping: right/left=0°, up/down=90°, down-left/up-right=0-90°, down-right/up-left=90-180°
            float atan2Deg = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            float z = atan2Deg >= 0 ? atan2Deg : atan2Deg + 180f;
            transform.rotation = Quaternion.Euler(0f, 0f, z);
        }

        public void PlayRotation(AnimationCurve rotationCurve, float rotationDuration, int totalLoops)
        {
            StopRotation();
            float initialZ = transform.eulerAngles.z;
            _rotationCoroutine = StartCoroutine(RotateRoutine(rotationCurve, rotationDuration, totalLoops, initialZ));
            SwitchLaserColliders(true);
        }

        private static float NormalizeAngle(float a)
        {
            while (a < 0) a += 360f;
            while (a >= 360f) a -= 360f;
            return a;
        }
        public void StopRotation()
        {
            SwitchLaserColliders(false);
            IsRotating = false;
            if (_rotationCoroutine == null) return;
            StopCoroutine(_rotationCoroutine);
            _rotationCoroutine = null;
        }
        private void SwitchLaserColliders(bool state)
        {
            foreach (var laserCollider in laserCollidersGameObjects)
            {
                if (laserCollider != null)
                {
                    laserCollider.SetActive(state);
                }
            }
        }

        private IEnumerator RotateRoutine(AnimationCurve rotationCurve, float rotationDuration, int totalLoops, float initialZ)
        {
            if (rotationDuration <= 0f) yield break;
            IsRotating = true;
            float elapsed = 0f;
            float targetTotalDegrees = 360f * totalLoops;

            while (elapsed < rotationDuration)
            {
                float dt = Time.deltaTime;
                elapsed += dt;
                float t = Mathf.Clamp01(elapsed / rotationDuration);
                float curveValue = rotationCurve.Evaluate(t);
                float desiredTotalRotation = targetTotalDegrees * curveValue;
                float currentZ = NormalizeAngle(initialZ + desiredTotalRotation);
                transform.rotation = Quaternion.Euler(0f, 0f, currentZ);

                yield return null;
            }

            transform.rotation = Quaternion.Euler(0f, 0f, NormalizeAngle(initialZ + targetTotalDegrees));
            IsRotating = false;
            _rotationCoroutine = null;
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