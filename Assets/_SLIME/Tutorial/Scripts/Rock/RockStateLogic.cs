using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;



namespace _SLIME.Tutorial
{
    
    [Serializable]
    public struct RockStateDeps
    {
        public Animator rockAnimator;
        public Animator ExplosionAnimator;
        public GameObject rockBackground;
        public GameObject happyBossBackground;
        public GameObject slimeGameObject;
        public Camera mainCamera;
    }
    [Serializable]
    public struct RockStateSet
    {
        public float startCameraSize;
        public float zoomOutAmount;
        public Ease zoomEase;
        public float zoomSpeed;
    }


    public class RockStateLogic : ITutorialStateLogic
    {
        private RockStateDeps _rockStateDeps;
        private RockStateSet _rockStateSet;
        private List<Func<IEnumerator>> _routines;
        private int _currentRoutine = 0;
        private bool _joystickMoved = false;

        public RockStateLogic(RockStateDeps rockStateDeps,
            RockStateSet rockStateSet)
        {

            _rockStateDeps = rockStateDeps;
            _rockStateSet = rockStateSet;
            Rock.JoystickMoved += OnJoystickMoved;
            
            _routines = new List<Func<IEnumerator>>()
            {
                () => firstRoutine(),
                () => secondRoutine(),
                () => thirdRoutine(),
                () => fourthRoutine()
            };
            _rockStateDeps.mainCamera.orthographicSize = _rockStateSet.startCameraSize;
        }

        public void OnDisable()
        {
            Rock.JoystickMoved -= OnJoystickMoved;
        }

        private void OnJoystickMoved()
        {
            _joystickMoved = true;
        }
        
        
        public IEnumerator Start()
        {
            while (_currentRoutine < _routines.Count)
            {
                yield return null;
                if (_joystickMoved)
                {
                    _joystickMoved = false;
                    yield return _routines[_currentRoutine]();
                    _currentRoutine++;
                }
            }
            
        }
        
        #region routines
        private IEnumerator firstRoutine()
        {
            yield return AnimateCameraRoutine("TriggerPhase2", "Phase2");
        }

        private IEnumerator secondRoutine()
        {
            yield return AnimateCameraRoutine("TriggerPhase3", "Phase3");
        }

        private IEnumerator thirdRoutine()
        {
            yield return AnimateCameraRoutine("TriggerPhase4", "Phase4");
        }

        private IEnumerator fourthRoutine()
        {
            yield return AnimateCameraRoutine("TriggerSlime", "Phase4ToSlime");
            _rockStateDeps.rockAnimator.gameObject.SetActive(false);
            yield return AnimateExplosion();
            
        }

        private IEnumerator AnimateExplosion()
        {
            
            yield return TriggerAndWaitForState(_rockStateDeps.ExplosionAnimator, "In stone boom", "In stone boom");
            _rockStateDeps.rockBackground.SetActive(false);
            _rockStateDeps.slimeGameObject.SetActive(true);
            _rockStateDeps.happyBossBackground.SetActive(true);
            yield return TriggerAndWaitForState(_rockStateDeps.ExplosionAnimator, "boom out", "boom out");
            
        }

        #endregion


        /// <summary>
        /// Generic routine for spawning a floating object with animation trigger
        /// </summary>
        private IEnumerator AnimateCameraRoutine(string triggerName, string stateName)
        {
            _rockStateDeps.mainCamera.DOOrthoSize(_rockStateDeps.mainCamera.orthographicSize + _rockStateSet.zoomOutAmount, _rockStateSet.zoomSpeed).SetEase(_rockStateSet.zoomEase);
            yield return TriggerAndWaitForState(_rockStateDeps.rockAnimator, triggerName, stateName);
        }

        #region Animator Utilities

        /// <summary>
        /// Triggers an animation and waits until it completes
        /// </summary>
        private IEnumerator TriggerAndWaitForState(Animator animator, string triggerName, string targetStateName, int layer = 0)
        {
      
            animator.SetTrigger(triggerName);

            yield return null;

            while (!animator.GetCurrentAnimatorStateInfo(layer).IsName(targetStateName))
            {
                yield return null;
            }
            
            while (animator.GetCurrentAnimatorStateInfo(layer).IsName(targetStateName) && 
                   animator.GetCurrentAnimatorStateInfo(layer).normalizedTime < 1.0f)
            {
                yield return null;
            }
        }
        #endregion

    }
}