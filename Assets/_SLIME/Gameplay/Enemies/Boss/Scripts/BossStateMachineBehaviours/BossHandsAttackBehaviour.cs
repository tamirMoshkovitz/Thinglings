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
        private List<HandWrapper> _allHands = new List<HandWrapper>();

        private class HandWrapper
        {
            public GameObject Root { get; private set; }
            public BossHandAttackLogic Logic { get; private set; }
            public bool InCurrentSequence { get; set; }

            public HandWrapper(GameObject obj) { 
                Root = obj; 
                Logic = obj.GetComponentInChildren<BossHandAttackLogic>();
            }
        }

        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            base.OnStateEnter(animator, stateInfo, layerIndex);
            Data.BossCloseState();
            
            _allHands.Clear();
            _allHands.AddRange(Data.topLeftHands.Select(h => new HandWrapper(h)));
            _allHands.AddRange(Data.topRightHands.Select(h => new HandWrapper(h)));
            _allHands.AddRange(Data.bottomLeftHands.Select(h => new HandWrapper(h)));
            _allHands.AddRange(Data.bottomRightHands.Select(h => new HandWrapper(h)));
            // Add specials for the random pool
            _allHands.AddRange(Data.specialLeftHandSplines.Select(h => new HandWrapper(h)));
            _allHands.AddRange(Data.specialRightHandSplines.Select(h => new HandWrapper(h)));

            ForceStopAllHands();
            _smashRoutine = Data.StartCoroutine(MemorySmashRoutine(animator));
        }

        private IEnumerator MemorySmashRoutine(Animator animator)
        {
            var config = BossBrain.bossConfigurations.HandsAttack;
            float warningSpeed = config.phaseSpeedMultiplier;
            List<HandWrapper> sequence = new List<HandWrapper>();

            // --- 1. GUARANTEE ONE FROM EACH CORNER ---
            AddGuaranteedHand(sequence, Data.topLeftHands);
            AddGuaranteedHand(sequence, Data.topRightHands);
            AddGuaranteedHand(sequence, Data.bottomLeftHands);
            AddGuaranteedHand(sequence, Data.bottomRightHands);

            sequence = sequence.OrderBy(x => Random.value).ToList();

            // --- 2. FILL REMAINING SLOTS ---
            while (sequence.Count < config.totalHandsToUse)
            {
                var extra = _allHands.FirstOrDefault(h => !h.InCurrentSequence && !h.Root.activeSelf);
                if (extra != null) {
                    extra.InCurrentSequence = true;
                    sequence.Add(extra);
                } else break;
            }

            // --- 3. REVEAL PHASE ---
            foreach (var hand in sequence)
            {
                if (Data.WaterStateActivated) yield break;
                hand.Root.SetActive(true);
                hand.Logic.ResetHand(); // Snap to 0 while invisible
                yield return Data.StartCoroutine(hand.Logic.PlayWarningSequence(warningSpeed));
                yield return new WaitForSeconds(0.15f / warningSpeed);
            }

            yield return new WaitForSeconds(0.5f / warningSpeed);

            // --- 4. STRIKE PHASE ---
            foreach (var hand in sequence)
            {
                if (Data.WaterStateActivated) break;
                Data.StartCoroutine(ExecuteAndCleanup(hand)); 
                yield return new WaitForSeconds(config.handCooldown / warningSpeed);
            }

            yield return new WaitUntil(() => AllFinished() || Data.WaterStateActivated);
            animator.SetTrigger(AttackFinished);
        }

        private void AddGuaranteedHand(List<HandWrapper> sequence, List<GameObject> pool)
        {
            var available = _allHands.Where(h => pool.Contains(h.Root) && !h.InCurrentSequence).ToList();
            if (available.Count > 0)
            {
                var chosen = available[Random.Range(0, available.Count)];
                chosen.InCurrentSequence = true;
                sequence.Add(chosen);
            }
        }

        private IEnumerator ExecuteAndCleanup(HandWrapper hand)
        {
            yield return hand.Logic.PlayAttack();
            hand.Root.SetActive(false); // Disappear immediately at the end
            hand.InCurrentSequence = false;
        }

        private bool AllFinished() => _allHands.All(h => !h.Root.activeSelf);
        private void ForceStopAllHands() => _allHands.ForEach(h => { h.Root.SetActive(false); h.InCurrentSequence = false; });
    }
}