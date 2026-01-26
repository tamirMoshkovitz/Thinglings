using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using _SLIME.Generics; 

namespace _SLIME.GameLoop
{
    public enum SceneType
    {
        StartScene,
        StartSceneAfterDeath,
        BossFinalBattleScene,
        ManagerScene
    }
    
    public class SceneLoader : MonoSingleton<SceneLoader>
    {
        [Header("UI Configuration")]
        [SerializeField] private GameObject _loadingScreenPrefab; 
        [SerializeField] private float _fadeDuration = 0.5f;
        
        

        private static readonly Dictionary<SceneType, string> sceneTypeToIndex = new Dictionary<SceneType, string>();
        private static readonly Dictionary<string, SceneType> indexToSceneType = new Dictionary<string,SceneType>();
        
        private static readonly List<Action> _everyTimeActions = new();
        private static readonly List<Action> _oneTimeActions = new();
        
        public static SceneType CurrentSceneType => indexToSceneType[SceneManager.GetActiveScene().name];

        static SceneLoader()
        {
            sceneTypeToIndex.Add(SceneType.StartScene, "FirstScene");
            sceneTypeToIndex.Add(SceneType.BossFinalBattleScene, "BossFinalBattle");
            sceneTypeToIndex.Add(SceneType.StartSceneAfterDeath, "FirstSceneAfterDamdge");
            sceneTypeToIndex.Add(SceneType.ManagerScene, "ManagerScene");
            
            indexToSceneType.Add("FirstScene", SceneType.StartScene);
            indexToSceneType.Add("BossFinalBattle", SceneType.BossFinalBattleScene);
            indexToSceneType.Add("FirstSceneAfterDamdge", SceneType.StartSceneAfterDeath);
            indexToSceneType.Add("ManagerScene", SceneType.ManagerScene);
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void AutoLoadManagerScene()
        {
            SceneManager.LoadSceneAsync(sceneTypeToIndex[SceneType.ManagerScene], LoadSceneMode.Additive);
        }
        

        protected override void Awake()
        {
            transform.SetParent(null); 
            base.Awake(); 
            if (Instance != this) return;
        }
        
        public static void AddEveryTimeAction(Action action) => _everyTimeActions.Add(action);
        public static void AddOneTimeAction(Action action) => _oneTimeActions.Add(action);

        private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            foreach (var action in _everyTimeActions) action?.Invoke();
            foreach (var action in _oneTimeActions) action?.Invoke();
            _oneTimeActions.Clear();
        }
        
        // --- Load Logic ---
        
        public static void LoadScene(SceneType sceneType, Action callback = null)
        {
            if (callback != null) AddOneTimeAction(callback);
            
            Instance.StartCoroutine(Instance.ProcessSceneLoading(sceneTypeToIndex[sceneType]));
        }

        private IEnumerator ProcessSceneLoading(string newSceneName)
        {
            GameObject loadingScreenInstance = Instantiate(_loadingScreenPrefab, transform);
            Image fadeImage = loadingScreenInstance.GetComponentInChildren<Image>();

            yield return StartCoroutine(Fade(fadeImage, 0, 1));

            Scene currentScene = SceneManager.GetActiveScene();

            
            yield return SceneManager.UnloadSceneAsync(currentScene);
            

            AsyncOperation loadOp = SceneManager.LoadSceneAsync(newSceneName, LoadSceneMode.Additive);
            while (!loadOp.isDone) yield return null;

            Scene newScene = SceneManager.GetSceneByName(newSceneName);
            SceneManager.SetActiveScene(newScene);

            Time.timeScale = 0f;
            yield return StartCoroutine(Fade(fadeImage, 1, 0));
            Time.timeScale = 1f;

            Destroy(loadingScreenInstance);
        }

        private IEnumerator Fade(Image targetImage, float startAlpha, float endAlpha)
        {
            float time = 0;
            Color color = targetImage.color;
            color.a = startAlpha;
            targetImage.color = color;

            while (time < _fadeDuration)
            {
                time += Time.unscaledDeltaTime; 
                color.a = Mathf.Lerp(startAlpha, endAlpha, time / _fadeDuration);
                targetImage.color = color;
                yield return null;
            }
            color.a = endAlpha;
            targetImage.color = color;
        }
    }
}