using System.Collections;
using _SLIME.BaseScripts;
using _SLIME.Core.Audio.FMOD.Scripts;
using _SLIME.Slime;
using FMODUnity;
using UnityEngine;
using UnityEngine.Splines;

namespace _SLIME.Boss
{
    public class BossHandAttackLogic : ProjectMonoBehavior
    {
        [Header("References")] 
        [SerializeField] private SplineAnimate splineAnimate;
        [SerializeField] private GameObject warningVisual;
        [SerializeField] private AttackVisualWarning warningScript;

        [Header("Animation Settings")] 
        [SerializeField] private AnimationCurve easeCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

        [Header("Rotation Settings")]
        [SerializeField] private bool useRefinedRotation = true;
        [SerializeField] private Vector3 startRotation = new Vector3(0, 0, 0);
        [SerializeField] private Vector3 endRotation = new Vector3(0, 180, 0);
        
        [Header("Audio")]
        [SerializeField] private EventReference handAttackSFX;

        public bool IsAttacking { get; private set; }

        private void Awake()
        {
            splineAnimate.PlayOnAwake = false;
            splineAnimate.Pause(); 
        }

        public void ResetHand()
        {
            splineAnimate.NormalizedTime = 0f;
            if (useRefinedRotation) transform.rotation = Quaternion.Euler(startRotation);
            warningVisual.SetActive(false);
            IsAttacking = false;
        }

        public IEnumerator PlayWarningSequence(float warningSpeed)
        {
            warningVisual.SetActive(true);
            warningScript.SetSpeed(warningSpeed);
            warningScript.Play();

            yield return new WaitUntil(() => !warningScript.IsAnimationPlaying);
            warningVisual.SetActive(false);
        }

        public IEnumerator PlayAttack()
        {
            IsAttacking = true;
            SFXPlayer.Play(handAttackSFX);

            float timer = 0f;
            float duration = BossBrain.bossConfigurations.HandsAttack.handAttackDuration;

            Quaternion rotationFrom = Quaternion.Euler(startRotation);
            Quaternion rotationTo = Quaternion.Euler(endRotation);

            while (timer < duration)
            {
                timer += Time.deltaTime;
                float progress = easeCurve.Evaluate(Mathf.Clamp01(timer / duration));

                splineAnimate.NormalizedTime = progress;

                if (useRefinedRotation)
                    transform.rotation = Quaternion.Slerp(rotationFrom, rotationTo, progress);

                yield return null;
            }

            splineAnimate.NormalizedTime = 1f;
            IsAttacking = false;
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