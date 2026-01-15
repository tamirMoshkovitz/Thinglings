using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace _SLIME.Boss
{
    public class BossHandsAttackBehaviour : BossBaseBehaviour
    {
        private static readonly int AttackFinished = Animator.StringToHash("AttackFinished");
        private Coroutine _smashRoutine;
        
        private List<HandWrapper> _leftHands;
        private List<HandWrapper> _rightHands;

        private class HandWrapper
        {
            public GameObject Root { get; private set; }
            public BossHandAttackLogic Logic { get; private set; }

            public HandWrapper(GameObject obj)
            {
                Root = obj;
                Logic = obj.GetComponentInChildren<BossHandAttackLogic>();
            }

            public void Activate(float duration)
            {
                Root.SetActive(true);
            }

            public void Deactivate()
            {
                if (Root.activeSelf) Root.SetActive(false);
            }
        }

        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            base.OnStateEnter(animator, stateInfo, layerIndex);
            TotalAttacksPreformed++;

            _leftHands = Data.leftHandSplines.Select(h => new HandWrapper(h)).ToList();
            _rightHands = Data.rightHandSplines.Select(h => new HandWrapper(h)).ToList();

            ForceStopAllHands();
            _smashRoutine = Data.StartCoroutine(SmashRoutine(animator));
        }

        private IEnumerator SmashRoutine(Animator animator)
        {
            int totalAttacks = Data.bossConfigurations.HandsAttack.totalHandsToUse;
            float cooldown = Data.bossConfigurations.HandsAttack.handCooldown;
            float attackDuration = Data.bossConfigurations.HandsAttack.handAttackDuration;
            float warningDuration = Data.bossConfigurations.HandsAttack.handWarningDuration;

            // Optional: Add a buffer to the duration so we don't cut off the animation too early
            float totalLifeTime = warningDuration + attackDuration + 0.1f;

            for (int i = 0; i < totalAttacks; i++)
            {
                List<HandWrapper> handsToFire = new List<HandWrapper>();

                if (Data.bossConfigurations.HandsAttack.useBothHands)
                {
                    AddRandomHandTo(handsToFire, _leftHands);
                    AddRandomHandTo(handsToFire, _rightHands);
                }
                else
                {
                    bool isLeft = Random.value > 0.5f;
                    AddRandomHandTo(handsToFire, isLeft ? _leftHands : _rightHands);
                }

                foreach (var hand in handsToFire)
                {
                    hand.Activate(attackDuration);

                    Data.StartCoroutine(MonitorAndDisableHand(hand, totalLifeTime));
                }

                yield return new WaitForSeconds(Mathf.Max(cooldown, 0.1f));
            }

            // Wait until all hands are essentially "done" before exiting state
            yield return new WaitUntil(() => AllHandsFinished());

            animator.SetTrigger(AttackFinished);
        }

        // This mini-routine handles turning off a specific hand after its job is done
        private IEnumerator MonitorAndDisableHand(HandWrapper hand, float duration)
        {
            // Wait for the total duration (Warning + Attack)
            yield return new WaitForSeconds(duration);
            
            // Turn it off
            hand.Deactivate();
        }

        private bool AllHandsFinished()
        {
            // Check if any hand gameobjects are still active
            return !_leftHands.Any(h => h.Root.activeSelf) && !_rightHands.Any(h => h.Root.activeSelf);
        }

        private void AddRandomHandTo(List<HandWrapper> targetList, List<HandWrapper> sourceList)
        {
            if (sourceList == null || sourceList.Count == 0) return;
            
            // Only pick hands that are currently OFF
            var availableHands = sourceList.Where(h => !h.Root.activeSelf).ToList();
            
            if (availableHands.Count > 0)
            {
                targetList.Add(availableHands[Random.Range(0, availableHands.Count)]);
            }
        }

        private void ForceStopAllHands()
        {
            _leftHands?.ForEach(h => h.Deactivate());
            _rightHands?.ForEach(h => h.Deactivate());
        }

        public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            if (_smashRoutine != null) Data.StopCoroutine(_smashRoutine);
            ForceStopAllHands();
        }
    }
}