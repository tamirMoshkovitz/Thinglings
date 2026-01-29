using System.Collections;
using _SLIME.GameLoop;
using _SLIME.Slime;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;

namespace _SLIME.Tutorial
{
    [System.Serializable]
    public struct SceneMoveToFinalBattleStateDeps
    {
        [Tooltip("Root of transition UI (Animator on it). Moved to DontDestroyOnLoad before unload, destroyed at end of transition.")]
        public Animator transitionAnimator;
        public GameObject tutorialBoss;
        public GameObject slime;
        public UnityEngine.InputSystem.PlayerInput slimeInput;
    }
    
    [System.Serializable]
    public struct SceneMoveToFinalBattleStateSet
    {
        public string transitionTriggerName;
        public string transitionAnimationStateName;
    }
    
    public class SceneMoveToFinalBattleLogic : ITutorialStateLogic
    {
        private SceneMoveToFinalBattleStateDeps _deps;
        private SceneMoveToFinalBattleStateSet _set;
        
        public SceneMoveToFinalBattleLogic(SceneMoveToFinalBattleStateDeps sceneMoveToFinalBattleStateDeps,
            SceneMoveToFinalBattleStateSet sceneMoveToFinalBattleStateSet)
        {
            _deps = sceneMoveToFinalBattleStateDeps;
            _set = sceneMoveToFinalBattleStateSet;
        }
        
        public void OnDisable()
        {
            // Cleanup if needed
        }
        
        public IEnumerator Start()
        {
            // DisableSlimeInput();
            // SetSlimeRenderersToUILayer();
            _deps.tutorialBoss.SetActive(false); 
            _deps.transitionAnimator.transform.SetParent(null, true);
            Object.DontDestroyOnLoad(_deps.transitionAnimator.gameObject);
            SlimeEvents.RemoveCameraShake();
            LoadFinalBattleSceneWithAnimationTransition();
            
            yield break;
        }
        
        private void SetSlimeRenderersToUILayer()
        {
            if (_deps.slime == null) return;
            int uiLayerId = SortingLayer.NameToID("UI");
            const int order = 10;
            foreach (var r in _deps.slime.GetComponentsInChildren<Renderer>(true))
            {
                r.sortingLayerID = uiLayerId;
                r.sortingOrder = order;
            }
            foreach (var g in _deps.slime.GetComponentsInChildren<SortingGroup>(true))
            {
                g.sortingLayerID = uiLayerId;
                g.sortingOrder = order;
            }
        }
        
        private void DisableSlimeInput()
        {
            if (_deps.slimeInput != null)
                _deps.slimeInput.enabled = false;
        }
        
        
        private void LoadFinalBattleSceneWithAnimationTransition()
        {
            var opts = new AnimationTransitionOptions
            {
                animator = _deps.transitionAnimator,
                triggerName = _set.transitionTriggerName,
                stateName = _set.transitionAnimationStateName
            };
            SceneLoader.LoadScene(SceneType.BossFinalBattleScene, SlimeEvents.AddCameraShake, opts);
        }
    }
}
