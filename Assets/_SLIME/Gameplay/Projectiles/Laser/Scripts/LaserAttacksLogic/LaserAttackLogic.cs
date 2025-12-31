using System;
using System.Collections;
using System.Collections.Generic;
using _SLIME.BaseScripts;
using UnityEngine;

namespace _SLIME.Laser
{
    public class LaserAttackLogic: ProjectMonoBehavior
    {
        private Coroutine _rotationCoroutine;
    
        [SerializeField] List<GameObject> laserCollidersGameObjects;
        public bool IsRotating { get; private set; }
        
        public bool HasFinishedAction { get; set;}
        
        private void OnDisable()
        {
            HasFinishedAction = false;
        }

        public void PlayRotation(AnimationCurve rotationCurve, float rotationDuration, int totalLoops)
        {
            StopRotation();
            _rotationCoroutine = StartCoroutine(RotateRoutine(rotationCurve, rotationDuration, totalLoops));
            SwitchLaserColliders(true);
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

        private IEnumerator RotateRoutine(AnimationCurve rotationCurve, float rotationDuration, int totalLoops)
        {
            if (rotationDuration <= 0f) yield break;
            IsRotating = true;
            float elapsed = 0f;
            float targetTotalDegrees = 360f * totalLoops;
            float currentRotationProgress = 0f;
        
            while (elapsed < rotationDuration)
            {
                float dt = Time.deltaTime;
                elapsed += dt;
                float t = Mathf.Clamp01(elapsed / rotationDuration);
                float curveValue = rotationCurve.Evaluate(t);
                float desiredTotalRotation = targetTotalDegrees * curveValue;
                float frameStep = desiredTotalRotation - currentRotationProgress;
                transform.Rotate(Vector3.forward, frameStep, Space.Self);
                currentRotationProgress = desiredTotalRotation;

                yield return null;
            }

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