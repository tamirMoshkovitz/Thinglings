using System;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

namespace _SLIME.GameLoop
{
    public enum SceneType
    {
        StartScene,
        BossBecomesMadScene,
        BossChaseScene,
        BossFinalBattleScene,
    }
    
    public static class SceneLoader
    {
        private static readonly Dictionary<SceneType, string> sceneTypeToIndex = new Dictionary<SceneType, string>();
        private static readonly Dictionary<string, SceneType> indexToSceneType = new Dictionary<string,SceneType>();
        
        private static readonly List<Action> _everyTimeActions = new();
        
        private static readonly List<Action> _oneTimeActions = new();
        
        public static SceneType CurrentSceneType => indexToSceneType[SceneManager.GetActiveScene().name];
        
        static SceneLoader()
        {
            sceneTypeToIndex.Add(SceneType.StartScene, "FirstScene");
            sceneTypeToIndex.Add(SceneType.BossBecomesMadScene, "BossBecomesMad");
            sceneTypeToIndex.Add(SceneType.BossChaseScene, "BossChase");
            sceneTypeToIndex.Add(SceneType.BossFinalBattleScene, "BossFinalBattle");
            indexToSceneType.Add("StartScene", SceneType.StartScene);
            indexToSceneType.Add("BossBecomesMad", SceneType.BossBecomesMadScene);
            indexToSceneType.Add("BossChase", SceneType.BossChaseScene);
            indexToSceneType.Add("BossFinalBattle", SceneType.BossFinalBattleScene);
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        
        /// <summary>
        /// Register a callback that will run on *every* scene load.
        /// </summary>
        public static void AddEveryTimeAction(Action action)
        {
            _everyTimeActions.Add(action);
        }
        
        
        /// <summary>
        /// Register a callback that will run on *the next* scene load.
        /// </summary>
        public static void AddOneTimeAction(Action action)
        {
            _oneTimeActions.Add(action);
        }


        private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            foreach (var action in _everyTimeActions)
            {
                action?.Invoke();
            }
            
            foreach (var action in _oneTimeActions)
            {
                action?.Invoke();
            }
            _oneTimeActions.Clear();
        }
        
        public static void LoadScene(SceneType sceneType, Action callback = null)
        {
            if (callback != null)
                AddOneTimeAction(callback);
            
            SceneManager.LoadSceneAsync(sceneTypeToIndex[sceneType]);
        }
    }
}