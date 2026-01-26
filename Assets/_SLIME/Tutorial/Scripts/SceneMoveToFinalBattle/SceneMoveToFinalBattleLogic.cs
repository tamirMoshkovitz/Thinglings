using System.Collections;
using _SLIME.GameLoop;
using UnityEngine;

namespace _SLIME.Tutorial
{
    [System.Serializable]
    public struct SceneMoveToFinalBattleStateDeps
    {
        public Animator transitionAnimator;
        public GameObject tutorialBoss;
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
            _deps.tutorialBoss.SetActive(false);
            TriggerTransitionAnimation();
            yield return WaitForAnimationEnd();
            LoadFinalBattleScene();
        }
        
        private void TriggerTransitionAnimation()
        {
            _deps.transitionAnimator.SetTrigger(_set.transitionTriggerName);
        }
        
        private IEnumerator WaitForAnimationEnd()
        {
            while (!_deps.transitionAnimator.GetCurrentAnimatorStateInfo(0).IsName(_set.transitionAnimationStateName))
            {
                yield return null;
            }
            
            while (_deps.transitionAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1.0f)
            {
                yield return null;
            }
        }
        
        private void LoadFinalBattleScene()
        {
            SceneLoader.LoadScene(SceneType.BossFinalBattleScene);
        }
    }
}
