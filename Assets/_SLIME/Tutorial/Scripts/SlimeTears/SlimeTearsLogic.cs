using System;
using System.Collections;
using _SLIME.GameLoop;
using UnityEngine;

namespace _SLIME.Tutorial
{
    [System.Serializable]
    public struct SlimeTearsStateDeps
    {
        public Animator sorcererAnimator;
        public Animator transitionAnimator;
        public Renderer transitionRenderer;
        public GameObject sorcerer;
        public UnityEngine.InputSystem.PlayerInput slimeInput;
        public GameObject tutorialBoss;
        public Animator sketchStretchAnimator;
    }
    
    [System.Serializable]
    public struct SlimeTearsStateSet
    {
        public string sorcererTriggerName;
        public string sorcererWaitForStateName;
        public string transitionTriggerName;
        public string transitionWaitForStateName;
    }
    
    public class SlimeTearsLogic : ITutorialStateLogic
    {
        private SlimeTearsStateDeps _slimeTearsStateDeps;
        private SlimeTearsStateSet _slimeTearsStateSet;
        private bool _eventTriggered = false;
        
        public SlimeTearsLogic(SlimeTearsStateDeps slimeTearsStateDeps,
            SlimeTearsStateSet slimeTearsStateSet)
        {
            _slimeTearsStateDeps = slimeTearsStateDeps;
            _slimeTearsStateSet = slimeTearsStateSet;
            Slime.SlimeEvents.SlimeTears += OnSlimeTears;
        }

        public void OnDisable()
        {
            Slime.SlimeEvents.SlimeTears -= OnSlimeTears;
        }
        
        private void DisableSlimeInput()
        {
            _slimeTearsStateDeps.slimeInput.enabled = false; 
        }
        
        private void EnableSlimeInput()
        {
            _slimeTearsStateDeps.slimeInput.enabled = true;
        }
        
        public IEnumerator Start()
        {
            yield return WaitForSlimeTears();
            // DisableSlimeInput(); 
            yield return PlayAnimations();
            // EnableSlimeInput();
        }

        private IEnumerator PlayAnimations()
        {
            _slimeTearsStateDeps.sorcererAnimator.SetTrigger(_slimeTearsStateSet.sorcererTriggerName);
            yield return WaitForAnimationState(_slimeTearsStateDeps.sorcererAnimator, 
                _slimeTearsStateSet.sorcererWaitForStateName);
            _slimeTearsStateDeps.transitionRenderer.sortingLayerName = "Stage";
            _slimeTearsStateDeps.sorcerer.SetActive(false);
            _slimeTearsStateDeps.transitionAnimator.SetTrigger(_slimeTearsStateSet.transitionTriggerName);
            yield return WaitForAnimationState(_slimeTearsStateDeps.transitionAnimator, 
                _slimeTearsStateSet.transitionWaitForStateName);
            _slimeTearsStateDeps.tutorialBoss.SetActive(true);
            
        }

        private void OnSlimeTears()
        {
            _eventTriggered = true;
            GameEvents.FmodPhaseTwo?.Invoke();
        }
        
        private IEnumerator WaitForSlimeTears()
        {
            _slimeTearsStateDeps.sketchStretchAnimator.SetTrigger("stretch");
            while (!_eventTriggered)
            {
                yield return null;
            }
            _slimeTearsStateDeps.sketchStretchAnimator.gameObject.SetActive(false);
        }
        
        private IEnumerator WaitForAnimationState(Animator animator, string stateName)
        {
            yield return null;
            
            AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
            while (!stateInfo.IsName(stateName))
            {
                yield return null;
                stateInfo = animator.GetCurrentAnimatorStateInfo(0);
            }
        }
    }
}
