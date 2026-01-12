using System;
using System.Collections;
using DG.Tweening;
using UnityEngine;

namespace _SLIME.Tutorial
{
    [System.Serializable]
    public struct SlimeConnectsStateDeps
    {
        public Animator sorcererAnimator;
        public Transform slime1;
        public Transform slime2;
        public UnityEngine.InputSystem.PlayerInput slimeInput;
    }
    
    [System.Serializable]
    public struct SlimeConnectsStateSet
    {
        public string excitedTriggerName;
        public string waitForStateName;
        public float finalDistance;
        public float moveSpeed;
        public AnimationCurve moveEase;
    }
    
    public class SlimeConnectsLogic
    {
        private SlimeConnectsStateDeps _slimeConnectsStateDeps;
        private SlimeConnectsStateSet _slimeConnectsStateSet;
        
        public SlimeConnectsLogic(SlimeConnectsStateDeps slimeConnectsStateDeps,
            SlimeConnectsStateSet slimeConnectsStateSet)
        {
            _slimeConnectsStateDeps = slimeConnectsStateDeps;
            _slimeConnectsStateSet = slimeConnectsStateSet;
        }
        
        public IEnumerator Start()
        {
            DisableSlimeInput();
            TriggerSorcererExcited();
            yield return WaitForAnimationToEnd();
            yield return MoveSlimesTogether();
            EnableSlimeInput();
        }
        
        private void DisableSlimeInput()
        {
            _slimeConnectsStateDeps.slimeInput.enabled = false; 
        }
        
        private void EnableSlimeInput()
        {
            _slimeConnectsStateDeps.slimeInput.enabled = true;
        }
        
        private void TriggerSorcererExcited()
        {
            _slimeConnectsStateDeps.sorcererAnimator.SetTrigger(_slimeConnectsStateSet.excitedTriggerName);
        }
        
        private IEnumerator WaitForAnimationToEnd()
        {
            yield return null;
            
            AnimatorStateInfo stateInfo = _slimeConnectsStateDeps.sorcererAnimator.GetCurrentAnimatorStateInfo(0);
            while (!stateInfo.IsName(_slimeConnectsStateSet.waitForStateName))
            {
                yield return null;
                stateInfo = _slimeConnectsStateDeps.sorcererAnimator.GetCurrentAnimatorStateInfo(0);
            }
        }
        
        private IEnumerator MoveSlimesTogether()
        {
            Vector3 slime1Pos = _slimeConnectsStateDeps.slime1.position;
            Vector3 slime2Pos = _slimeConnectsStateDeps.slime2.position;
            Vector3 midPoint = (slime1Pos + slime2Pos) / 2f;
            
            Vector3 direction = (slime2Pos - slime1Pos).normalized;
            float halfDistance = _slimeConnectsStateSet.finalDistance / 2f;
            
            Vector3 slime1Target = midPoint - direction * halfDistance;
            Vector3 slime2Target = midPoint + direction * halfDistance;
            
            Tween tween1 = _slimeConnectsStateDeps.slime1
                .DOMove(slime1Target, _slimeConnectsStateSet.moveSpeed)
                .SetEase(_slimeConnectsStateSet.moveEase);
            
            Tween tween2 = _slimeConnectsStateDeps.slime2
                .DOMove(slime2Target, _slimeConnectsStateSet.moveSpeed)
                .SetEase(_slimeConnectsStateSet.moveEase);
            
            yield return tween1.WaitForCompletion();
            yield return tween2.WaitForCompletion();
        }
    }
}