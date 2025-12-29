using System;
using System.Collections;
using _SLIME.BaseScripts;
using _SLIME.Boss;
using _SLIME.Slime;
using UnityEngine;
using UnityEngine.Splines;

namespace _SLIME.LittleBoss
{
    [Serializable]
    public struct LittleBossMovementSettings
    {
        [Header("Animation Settings")] 
        public float duration;
        public float waitUntilNextLoop;
        public AnimationCurve easeCurve;
        
        [Header("Rotation Settings")]
        public bool useRefinedRotation;
        public Vector3 startRotation;
        public Vector3 endRotation;
    }
    
    
    [Serializable]
    public struct LittleBossMovementRef
    {
        public SplineAnimate splineAnimate;
        public BaseBossSettings bossSettings;
    }
    
    public class LittleBossMovementLogic 
    {
        [Header("References")] 
        [SerializeField] private SplineAnimate splineAnimate;
        [SerializeField] private BaseBossSettings bossSettings;

        private LittleBossMovementSettings _set;

        public LittleBossMovementLogic()
        {
        }

        public void Start()
        {
            _set = bossSettings.LittleBossAttack;
        }

        private void OnEnable()
        {
            StartCoroutine(AnimateMoveAndRotate());
        }

        private IEnumerator AnimateMoveAndRotate()
        {
            while (true)
            {
                float timer = 0f;

                Quaternion rotationFrom = Quaternion.Euler(_set.startRotation);
                Quaternion rotationTo = Quaternion.Euler(_set.endRotation);

                while (timer < _set.duration)
                {
                    timer += Time.deltaTime;
                    float progress = timer / _set.duration;
                    float easedProgress = _set.easeCurve.Evaluate(progress);

                    splineAnimate.NormalizedTime = easedProgress;

                    if (_set.useRefinedRotation)
                    {
                        transform.rotation = Quaternion.Slerp(rotationFrom, rotationTo, easedProgress);
                    }

                    yield return null;
                }

                splineAnimate.NormalizedTime = 1f;
                if (_set.useRefinedRotation) transform.rotation = rotationTo;
                
                yield return new WaitForSeconds(_set.waitUntilNextLoop);
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            SlimeEvents.SlimeGetHit?.Invoke(other.gameObject);
        }

        public void SetDuration(float totalDuration)
        {
            _set.duration = totalDuration;
        }
    }
}