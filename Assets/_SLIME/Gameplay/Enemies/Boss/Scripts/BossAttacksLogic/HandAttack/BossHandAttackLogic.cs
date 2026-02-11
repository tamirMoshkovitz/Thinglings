using System.Collections;
using _SLIME.BaseScripts;
using _SLIME.Core.Audio.FMOD.Scripts;
using _SLIME.Slime;
using FMODUnity;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.Splines;

namespace _SLIME.Boss
{
    public class BossHandAttackLogic : ProjectMonoBehavior
    {
        [Header("System Settings")]
        [Tooltip("The Parent object here (the one the Boss Manager turns On/Off).")]
        [SerializeField] private GameObject rootObject; 

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
        
        [Header("Audio")]
        [SerializeField] private EventReference handAttackSfx;
        [SerializeField] private EventReference handWarningSfx;

        public bool IsAttacking { get; private set; }

        private Animator _warningAnimator;

        private void Awake()
        {
            splineAnimate.PlayOnAwake = false;
            splineAnimate.Loop = SplineAnimate.LoopMode.Once;

            if (warningVisual != null)
            {
                _warningAnimator = warningVisual.GetComponent<Animator>();
                if (_warningAnimator == null) 
                    _warningAnimator = warningVisual.GetComponentInChildren<Animator>();
            }
        }

        private void OnEnable()
        {
            splineAnimate.NormalizedTime = 0f;
            if (useRefinedRotation) transform.rotation = Quaternion.Euler(startRotation);
            
            StartCoroutine(AttackSequence());
        }

        private IEnumerator AttackSequence()
        {
            IsAttacking = true;

            if (warningVisual)
            {
                warningVisual.SetActive(true);
                SFXPlayer.Play(handWarningSfx);

                if (_warningAnimator)
                {
                    yield return null; 

                    AnimatorStateInfo info = _warningAnimator.GetCurrentAnimatorStateInfo(0);
                    yield return new WaitForSeconds(info.length);
                }
                else
                {
                    float fallbackDuration = BossBrain.bossConfigurations.HandsAttack.handWarningDuration;
                    yield return new WaitForSeconds(fallbackDuration);
                }

                warningVisual.SetActive(false);
            }

            SFXPlayer.Play(handAttackSfx);

            float timer = 0f;
            splineAnimate.Pause(); 

            Quaternion rotationFrom = Quaternion.Euler(startRotation);
            Quaternion rotationTo = Quaternion.Euler(endRotation);
            
            float duration = BossBrain.bossConfigurations.HandsAttack.handAttackDuration;

            while (timer < duration)
            {
                timer += Time.deltaTime;
                float rawProgress = Mathf.Clamp01(timer / duration);
                float easedProgress = easeCurve.Evaluate(rawProgress);

                splineAnimate.NormalizedTime = easedProgress;

                if (useRefinedRotation)
                {
                    transform.rotation = Quaternion.Slerp(rotationFrom, rotationTo, easedProgress);
                }

                yield return null;
            }

            splineAnimate.NormalizedTime = 1f;
            if (useRefinedRotation) transform.rotation = rotationTo;

            IsAttacking = false;
            rootObject.SetActive(false);
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