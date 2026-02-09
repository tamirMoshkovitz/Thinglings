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

        private List<HandWrapper> _specialLeftHands;  // Bottom Hands
        private List<HandWrapper> _specialRightHands; // Top Hands

        // Tracking toggles for alternation
        private bool _nextSpecialIsTop;
        private bool _isNextLeft;
        private float _firstFloatDistanceDuration;
        private float _firstFloatDistance;

        private class HandWrapper
        {
            public GameObject Root { get; private set; }
            
            public HandWrapper(GameObject obj)
            {
                Root = obj;
            }

            public void Activate()
            {
                if (Root != null) Root.SetActive(true);
            }

            public void Deactivate()
            {
                if (Root != null && Root.activeSelf) Root.SetActive(false);
            }

            public bool IsBusy => Root != null && Root.activeSelf; 
        }

        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            base.OnStateEnter(animator, stateInfo, layerIndex);
            Data.BossCloseState();
            TotalAttacksPreformed++;
            
            MakeBossNotFloating();

            // Initialize Wrappers
            _leftHands = Data.leftHandSplines.Select(h => new HandWrapper(h)).ToList();
            _rightHands = Data.rightHandSplines.Select(h => new HandWrapper(h)).ToList();
            _specialLeftHands = Data.specialBottomHands.Select(h => new HandWrapper(h)).ToList();
            _specialRightHands = Data.specialTopHands.Select(h => new HandWrapper(h)).ToList();

            // Determine starting sides randomly
            _isNextLeft = Random.value > 0.5f;
            _nextSpecialIsTop = Random.value > 0.5f;

            ForceStopAllHands();
            _smashRoutine = Data.StartCoroutine(SmashRoutine(animator));
        }

        private void MakeBossNotFloating()
        {
            _firstFloatDistance = Data.floatingAttributes.floatDistance;
            _firstFloatDistanceDuration = Data.floatingAttributes.duration;
            Data.floatingAttributes.floatDistance = 0f;
            Data.floatingAttributes.duration = 0f;
        }

        private IEnumerator SmashRoutine(Animator animator)
        {
            int totalAttacksToPerform = BossBrain.bossConfigurations.HandsAttack.totalHandsToUse;
            float cooldown = BossBrain.bossConfigurations.HandsAttack.handCooldown;

            int attacksLaunched = 0;
            int totalSpecialHandsPreformed = 0;

            while (attacksLaunched < totalAttacksToPerform)
            {
                // Global interrupt for phase transitions
                if (Data.WaterStateActivated)
                {
                    HandleExitCleanup(animator);
                    yield break;
                }

                bool isSpecialTurn = Data.centerDetector 
                                     && Data.centerDetector.IsReadyToFire 
                                     && totalSpecialHandsPreformed < BossBrain.bossConfigurations.HandsAttack.maxSpecialHands;
                
                HandWrapper handToFire = null;

                if (isSpecialTurn)
                {
                    // Logic: Alternates specifically between Top (Right List) and Bottom (Left List)
                    List<HandWrapper> specialList = _nextSpecialIsTop ? _specialRightHands : _specialLeftHands;
                    handToFire = GetRandomAvailableHand(specialList);

                    if (handToFire != null)
                    {
                        handToFire.Activate();
                        totalSpecialHandsPreformed++;
                        _nextSpecialIsTop = !_nextSpecialIsTop; // Alternate the special toggle
                        Data.centerDetector.ResetTrigger();
                    }
                }

                // If no special was fired this frame (or it wasn't a special turn), fire a normal hand
                if (handToFire == null)
                {
                    List<HandWrapper> normalList = _isNextLeft ? _leftHands : _rightHands;
                    handToFire = GetRandomAvailableHand(normalList);

                    if (handToFire != null)
                    {
                        handToFire.Activate();
                        attacksLaunched++;
                        _isNextLeft = !_isNextLeft; // Alternate the normal toggle
                    }
                }

                // Wait for the next attack interval
                if (handToFire != null)
                {
                    yield return new WaitForSeconds(Mathf.Max(cooldown, 0.1f));
                }
                else
                {
                    // If no hands were available at all, wait a frame and retry
                    yield return null;
                }
            }

            // Wait for all active hand animations to finish
            yield return new WaitUntil(() => AllHandsFinished() || Data.WaterStateActivated);

            HandleExitCleanup(animator);
        }

        private void HandleExitCleanup(Animator animator)
        {
            ForceStopAllHands();
            if (Data.centerDetector) Data.centerDetector.ResetTrigger();
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
            return available.Count > 0 ? available[Random.Range(0, available.Count)] : null;
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
            MakeBossFloatingAgain();
            if (Data.centerDetector != null) Data.centerDetector.ResetTrigger();
        }

        private void MakeBossFloatingAgain()
        {
            Data.floatingAttributes.floatDistance = _firstFloatDistance;
            Data.floatingAttributes.duration = _firstFloatDistanceDuration;
        }
    }
}