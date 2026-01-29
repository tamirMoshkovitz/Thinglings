using System.Collections;
using _SLIME.GameLoop;
using _SLIME.Slime;
using UnityEngine;

namespace _SLIME.Tutorial
{
    [System.Serializable]
    public struct SceneMoveToFinalBattleStateDeps
    {
        [Tooltip("Root of transition UI (Animator on it). Moved to DontDestroyOnLoad before unload, destroyed at end of transition.")]
        public Animator transitionAnimator;
        public GameObject tutorialBoss;
        public GameObject slime;
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
            _deps.slime.SetActive(false);
            _deps.tutorialBoss.SetActive(false); 
            _deps.transitionAnimator.transform.SetParent(null, true);
            Object.DontDestroyOnLoad(_deps.transitionAnimator.gameObject);
            SlimeEvents.RemoveCameraShake();
            LoadFinalBattleSceneWithAnimationTransition();
            
            yield break;
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
