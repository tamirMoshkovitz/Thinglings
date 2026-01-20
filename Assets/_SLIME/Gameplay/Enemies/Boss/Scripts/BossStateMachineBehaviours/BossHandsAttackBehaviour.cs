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

        private List<HandWrapper> _specialLeftHands;
        private List<HandWrapper> _specialRightHands;

        private class HandWrapper
        {
            public GameObject Root { get; private set; }
            
            public HandWrapper(GameObject obj)
            {
                Root = obj;
            }

            public void Activate()
            {
                Root.SetActive(true);
            }

            public void Deactivate()
            {
                if (Root.activeSelf) Root.SetActive(false);
            }

            public bool IsBusy => Root.activeSelf; 
        }

        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            base.OnStateEnter(animator, stateInfo, layerIndex);
            TotalAttacksPreformed++;

            _leftHands = Data.leftHandSplines.Select(h => new HandWrapper(h)).ToList();
            _rightHands = Data.rightHandSplines.Select(h => new HandWrapper(h)).ToList();
            _specialLeftHands = Data.specialLeftHandSplines.Select(h => new HandWrapper(h)).ToList();
            _specialRightHands = Data.specialRightHandSplines.Select(h => new HandWrapper(h)).ToList();

            ForceStopAllHands();
            _smashRoutine = Data.StartCoroutine(SmashRoutine(animator));
        }

        private IEnumerator SmashRoutine(Animator animator)
        {
            int totalAttacksToPerform = Data.bossConfigurations.HandsAttack.totalHandsToUse;
            float cooldown = Data.bossConfigurations.HandsAttack.handCooldown;

            int attacksLaunched = 0;
            bool isNextLeft = Random.value > 0.5f; 
            
            bool hasPerformedSpecial = false;

            while (attacksLaunched < totalAttacksToPerform)
            {

                bool isSpecialTurn = Data.centerDetector 
                                     && Data.centerDetector.IsReadyToFire 
                                     && !hasPerformedSpecial;
                
                HandWrapper handToFire = null;

                if (isSpecialTurn)
                {
                    List<HandWrapper> specialList = isNextLeft ? _specialLeftHands : _specialRightHands;
                    handToFire = GetRandomAvailableHand(specialList);
                }

                if (handToFire == null)
                {
                    List<HandWrapper> normalList = isNextLeft ? _leftHands : _rightHands;
                    handToFire = GetRandomAvailableHand(normalList);
                }

                if (handToFire != null)
                {
                    handToFire.Activate();

                    if (IsSpecialHand(handToFire))
                    {
                        hasPerformedSpecial = true;
                        Data.centerDetector.ResetTrigger();
                    }
                    else
                    {
                        attacksLaunched++;
                    }

                    isNextLeft = !isNextLeft;

                    yield return new WaitForSeconds(Mathf.Max(cooldown, 0.1f));
                }
                else
                {
                    yield return null;
                }
            }

            yield return new WaitUntil(AllHandsFinished);

            animator.SetTrigger(AttackFinished);
        }

        private bool IsSpecialHand(HandWrapper hand)
        {
            return _specialLeftHands.Contains(hand) || _specialRightHands.Contains(hand);
        }

        private bool AllHandsFinished()
        {
            bool standardBusy = _leftHands.Any(h => h.IsBusy) || _rightHands.Any(h => h.IsBusy);
            bool specialBusy = _specialLeftHands.Any(h => h.IsBusy) || _specialRightHands.Any(h => h.IsBusy);
            return !standardBusy && !specialBusy;
        }

        private HandWrapper GetRandomAvailableHand(List<HandWrapper> sourceList)
        {
            if (sourceList == null || sourceList.Count == 0) return null;
            
            var available = sourceList.Where(h => !h.IsBusy).ToList();
            if (available.Count > 0)
            {
                return available[Random.Range(0, available.Count)];
            }
            return null;
        }

        private void ForceStopAllHands()
        {
            _leftHands?.ForEach(h => h.Deactivate());
            _rightHands?.ForEach(h => h.Deactivate());
            _specialLeftHands?.ForEach(h => h.Deactivate());
            _specialRightHands?.ForEach(h => h.Deactivate());
        }

        public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            if (_smashRoutine != null) Data.StopCoroutine(_smashRoutine);
            ForceStopAllHands();
            if (Data.centerDetector != null) Data.centerDetector.ResetTrigger();
        }
    }
}