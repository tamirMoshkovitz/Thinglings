using System.Collections;
using _SLIME.BaseScripts;
using _SLIME.Slime;
using UnityEngine;
using UnityEngine.Splines;


namespace _SLIME.Boss
{
    public class BossHandAttackLogic : ProjectMonoBehavior
    {
        [Header("References")] 
        [SerializeField] private SplineAnimate splineAnimate;

        [Header("Animation Settings")] 
        [SerializeField] private float duration = 3f;

        [SerializeField] private AnimationCurve easeCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

        [Header("Rotation Settings")]
        [SerializeField] private bool useRefinedRotation = true;

        [SerializeField] private Vector3 startRotation = new Vector3(0, 0, 0);
        [SerializeField] private Vector3 endRotation = new Vector3(0, 180, 0);

        private void Awake()
        {
            splineAnimate.PlayOnAwake = false;
        }

        private void OnEnable()
        {
            StartCoroutine(AnimateMoveAndRotate());
        }

        private IEnumerator AnimateMoveAndRotate()
        {
            float timer = 0f;
            splineAnimate.Pause();

            Quaternion rotationFrom = Quaternion.Euler(startRotation);
            Quaternion rotationTo = Quaternion.Euler(endRotation);

            while (timer < duration)
            {
                timer += Time.deltaTime;
                float progress = timer / duration;
                float easedProgress = easeCurve.Evaluate(progress);

                splineAnimate.NormalizedTime = easedProgress;

                if (useRefinedRotation)
                {
                    transform.rotation = Quaternion.Slerp(rotationFrom, rotationTo, easedProgress);
                }

                yield return null;
            }

            splineAnimate.NormalizedTime = 1f;
            if (useRefinedRotation) transform.rotation = rotationTo;
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            var rig = collision.attachedRigidbody;
            if (rig && rig.TryGetComponent<IHealth>(out IHealth h))
            {
                h.TakeDamage(); 
            }
        }

        public void SetDuration(float totalDuration)
        {
            duration = totalDuration;
        }
    }
}