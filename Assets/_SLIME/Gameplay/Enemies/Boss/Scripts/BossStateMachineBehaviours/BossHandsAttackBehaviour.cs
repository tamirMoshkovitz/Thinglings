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
            _allHands.AddRange(Data.leftHandSplines.Select(h => new HandWrapper(h)));
            _allHands.AddRange(Data.rightHandSplines.Select(h => new HandWrapper(h)));
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
            bool isLeft = Random.value > 0.5f;

            bool hasUsedSpecial = false;

            for (int i = 0; i < config.totalHandsToUse; i++)
            {
                bool trySpecial = !hasUsedSpecial && Random.value < 0.2f;

                HandWrapper hand = PickHand(isLeft, trySpecial);
                if (hand != null) {
                    hand.InCurrentSequence = true;
                    sequence.Add(hand);
                    
                    if (IsSpecialHand(hand)) hasUsedSpecial = true;
                    
                    isLeft = !isLeft;
                }
            }

            foreach (var hand in sequence)
            {
                if (Data.WaterStateActivated) yield break;
                
                hand.Root.SetActive(true); 
                hand.Logic.ResetHand();
                
                yield return Data.StartCoroutine(hand.Logic.PlayWarningSequence(warningSpeed));
                yield return new WaitForSeconds(0.15f / warningSpeed);
            }

            yield return new WaitForSeconds(0.5f / warningSpeed);

            foreach (var hand in sequence)
            {
                if (Data.WaterStateActivated) break;
                Data.StartCoroutine(ExecuteAndCleanup(hand)); 
                yield return new WaitForSeconds(config.handCooldown / warningSpeed);
            }

            yield return new WaitUntil(() => AllFinished() || Data.WaterStateActivated);
            animator.SetTrigger(AttackFinished);
        }

        private IEnumerator ExecuteAndCleanup(HandWrapper hand)
        {
            yield return hand.Logic.PlayAttack();
            hand.Root.SetActive(false);
            hand.InCurrentSequence = false;
        }

        private HandWrapper PickHand(bool wantLeft, bool wantSpecial) {
            var available = _allHands.Where(h => !h.InCurrentSequence && !h.Root.activeSelf).ToList();
            
            var match = available.FirstOrDefault(h => {
                bool isSideMatch = wantLeft 
                    ? (Data.leftHandSplines.Contains(h.Root) || Data.specialLeftHandSplines.Contains(h.Root)) 
                    : (Data.rightHandSplines.Contains(h.Root) || Data.specialRightHandSplines.Contains(h.Root));
                
                bool isTypeMatch = wantSpecial 
                    ? (Data.specialLeftHandSplines.Contains(h.Root) || Data.specialRightHandSplines.Contains(h.Root))
                    : (!Data.specialLeftHandSplines.Contains(h.Root) && !Data.specialRightHandSplines.Contains(h.Root));
                
                return isSideMatch && isTypeMatch;
            });

            if (match == null && wantSpecial) return PickHand(wantLeft, false);

            return match ?? available.FirstOrDefault();
        }

        private bool IsSpecialHand(HandWrapper hand)
        {
            return Data.specialLeftHandSplines.Contains(hand.Root) || Data.specialRightHandSplines.Contains(hand.Root);
        }

        private bool AllFinished() => _allHands.All(h => !h.Root.activeSelf);

        private void ForceStopAllHands() => _allHands.ForEach(h => { h.Root.SetActive(false); h.InCurrentSequence = false; });
    }
}