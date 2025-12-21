using UnityEngine;
using System.Collections;

namespace _SLIME.Boss
{


    public class BossLaserAttackBehaviour : BossBaseBehaviour
    {
        private Coroutine _attackRoutine;

        override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            base.OnStateEnter(animator, stateInfo, layerIndex);

            if (data.laserArrayScript != null)
            {
                _attackRoutine = data.StartCoroutine(ExecuteAttackSequence(animator));
            }
        }

        private IEnumerator ExecuteAttackSequence(Animator animator)
        {
            var lasers = data.laserArrayScript;

            // 1. SETUP
            lasers.ResetVisuals();
            lasers.SetSpinning(true, data.laserRotationSpeed);

            // 2. GROW SEQUENCE (Explicit 0 -> Max)
            yield return data.StartCoroutine(lasers.PlayGrowSequence(data.laserGrowProfile, data.laserStaggerDelay));

            // 3. HOLD
            yield return new WaitForSeconds(data.laserActiveDuration);

            // 4. DISSOLVE SEQUENCE (Explicit Max -> 0)
            yield return data.StartCoroutine(lasers.PlayDissolveSequence(data.laserDissolveProfile,
                data.laserStaggerDelay));

            // 5. SAFETY WAIT
            yield return new WaitForSeconds(data.laserDissolveProfile.duration);

            // 6. FINISH
            lasers.SetSpinning(false, 0);
            lasers.ResetVisuals();
            lasers.gameObject.SetActive(false);
            animator.SetTrigger("Hide");
        }

        override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            if (_attackRoutine != null) data.StopCoroutine(_attackRoutine);

            if (data.laserArrayScript != null)
            {
                data.laserArrayScript.SetSpinning(false, 0);
                data.laserArrayScript.ResetVisuals();
                data.laserArrayScript.gameObject.SetActive(false);
            }
        }
    }
}