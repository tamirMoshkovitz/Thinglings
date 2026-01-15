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
        [SerializeField] private GameObject warningVisual; 
        [SerializeField] private BossBrain bossBrain;

        [Header("Animation Settings")] 
        [SerializeField] private AnimationCurve easeCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

        [Header("Rotation Settings")]
        [SerializeField] private bool useRefinedRotation = true;
        [SerializeField] private Vector3 startRotation = new Vector3(0, 0, 0);
        [SerializeField] private Vector3 endRotation = new Vector3(0, 180, 0);

        // PUBLIC FLAG for the State Machine to check
        public bool IsAttacking { get; private set; }

        private void Awake()
        {
            if (splineAnimate == null) splineAnimate = GetComponent<SplineAnimate>();
            splineAnimate.PlayOnAwake = false;
            splineAnimate.Loop = SplineAnimate.LoopMode.Once;

            if (warningVisual != null) warningVisual.SetActive(false);
        }

        private void OnEnable()
        {
            // Reset state
            splineAnimate.NormalizedTime = 0f;
            if (useRefinedRotation) transform.rotation = Quaternion.Euler(startRotation);
            
            StartCoroutine(AttackSequence());
        }

        private IEnumerator AttackSequence()
        {
            IsAttacking = true; // Mark as busy

            // --- Phase 1: Warning ---
            if (warningVisual != null) warningVisual.SetActive(true);
            yield return new WaitForSeconds(bossBrain.bossConfigurations.HandsAttack.handWarningDuration);
            if (warningVisual != null) warningVisual.SetActive(false);

            // --- Phase 2: Attack ---
            float timer = 0f;
            splineAnimate.Pause(); 

            Quaternion rotationFrom = Quaternion.Euler(startRotation);
            Quaternion rotationTo = Quaternion.Euler(endRotation);

            while (timer < bossBrain.bossConfigurations.HandsAttack.handAttackDuration)
            {
                timer += Time.deltaTime;
                float rawProgress = Mathf.Clamp01(timer / bossBrain.bossConfigurations.HandsAttack.handAttackDuration);
                float easedProgress = easeCurve.Evaluate(rawProgress);

                splineAnimate.NormalizedTime = easedProgress;

                if (useRefinedRotation)
                {
                    transform.rotation = Quaternion.Slerp(rotationFrom, rotationTo, easedProgress);
                }

                yield return null;
            }

            // --- Phase 3: Finish ---
            splineAnimate.NormalizedTime = 1f;
            if (useRefinedRotation) transform.rotation = rotationTo;

            IsAttacking = false; // Finished
            
            // SELF DISABLE: The hand turns itself off, simplifying the manager logic
            gameObject.SetActive(false); 
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            var rig = collision.attachedRigidbody;
            if (rig && rig.TryGetComponent<IHealth>(out IHealth h))
            {
                h.TakeDamage(); 
            }
        }
        
        private void OnDisable()
        {
            StopAllCoroutines();
            IsAttacking = false;
            if (warningVisual != null) warningVisual.SetActive(false);
        }
    }
}