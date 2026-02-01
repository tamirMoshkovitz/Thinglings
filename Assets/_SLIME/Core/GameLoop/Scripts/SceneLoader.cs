using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using _SLIME.Generics;
using _SLIME.Slime;

namespace _SLIME.GameLoop
{
    public enum SceneType
    {
        StartScene,
        StartSceneAfterDeath,
        BossFinalBattleScene,
        ManagerScene
    }

    /// <summary>
    /// Use for transition-to-final-battle: no fade, animation plays during load.
    /// Caller must DontDestroyOnLoad(animator.gameObject) before load; animator is destroyed at end of transition.
    /// Uses UnscaledTime so it keeps playing while Time.timeScale = 0.
    /// </summary>
    public struct AnimationTransitionOptions
    {
        public Animator animator;
        public string triggerName;
        public string stateName;
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
        
        private static Dictionary<SceneType, AsyncOperation> _preloadedScenes = new Dictionary<SceneType, AsyncOperation>();

        public static void StartBackgroundLoading(SceneType sceneType)
        {
            
            if (_preloadedScenes.ContainsKey(sceneType))
            {
                Debug.LogWarning($"Scene '{sceneType}' is already pre-loading.");
                return;
            }

            Instance.StartCoroutine(Instance.LoadSceneInBackground(sceneType));
        }

        private IEnumerator LoadSceneInBackground(SceneType sceneType)
        {
            string sceneName = sceneTypeToIndex[sceneType];
            Debug.Log($"[SceneLoader] Starting secure background load for: {sceneName}");

            Application.backgroundLoadingPriority = ThreadPriority.Low;

            AsyncOperation op = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
    
         
            op.allowSceneActivation = false;
            
            if (!_preloadedScenes.ContainsKey(sceneType))
            {
                _preloadedScenes.Add(sceneType, op);
            }

  
            while (!op.isDone)
            {
                
                if (op.progress >= 0.9f)
                {
                    Debug.Log($"[SceneLoader] Scene {sceneName} ready at 90% and waiting (Activation: {op.allowSceneActivation})");
                    break; 
                }
                yield return null;
            }
        }
        
        public static void CancelPreload(SceneType sceneType)
        {
            if (_preloadedScenes.ContainsKey(sceneType))
            {
                _preloadedScenes.Remove(sceneType);
            }
        }
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


        private void Start()
        {
            if(SceneManager.GetActiveScene().name != sceneTypeToIndex[SceneType.BossFinalBattleScene]) 
                StartBackgroundLoading(SceneType.BossFinalBattleScene);
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
        
        public static void LoadScene(SceneType sceneType, Action callback = null, AnimationTransitionOptions? transitionOptions = null)
        {
            if (callback != null) AddOneTimeAction(callback);
            
            Instance.StartCoroutine(Instance.ProcessSceneLoading(sceneType, transitionOptions));
        }

        private IEnumerator ProcessSceneLoading(SceneType newSceneType, AnimationTransitionOptions? transitionOptions = null)
        {
            string newSceneName = sceneTypeToIndex[newSceneType];
            bool useAnimationTransition = transitionOptions.HasValue &&
                transitionOptions.Value.animator != null &&
                !string.IsNullOrEmpty(transitionOptions.Value.stateName);

            if (!useAnimationTransition)
            {
                yield return StartCoroutine(ProcessSceneLoadingWithFade(newSceneName));
                yield break;
            }
            
            var opts = transitionOptions.Value;
            if (opts.animator.updateMode != AnimatorUpdateMode.UnscaledTime)
                opts.animator.updateMode = AnimatorUpdateMode.UnscaledTime;
            opts.animator.SetTrigger(opts.triggerName);
            yield return new WaitForSeconds(3f);
            opts.animator.GetComponent<SpriteRenderer>().sortingLayerID = SortingLayer.NameToID("UI");
            AsyncOperation loadOp;
            if (_preloadedScenes.TryGetValue(newSceneType, out AsyncOperation preloadedOp))
            {
                Debug.Log($"Using preloaded scene for type: {newSceneType}");
                loadOp = preloadedOp;
                _preloadedScenes.Remove(newSceneType); 
                loadOp.allowSceneActivation = true;
            }
            else
            {
                Debug.Log($"No preload found for {newSceneType}. Starting fresh load.");
                loadOp = SceneManager.LoadSceneAsync(newSceneName, LoadSceneMode.Additive);
            }
            Scene currentScene = SceneManager.GetActiveScene();
            yield return SceneManager.UnloadSceneAsync(currentScene);
            Application.backgroundLoadingPriority = ThreadPriority.Low;
            while (!loadOp.isDone) yield return null;
            Application.backgroundLoadingPriority = ThreadPriority.Normal;
            opts.animator.transform.position = Vector3.zero;
            Scene newScene = SceneManager.GetSceneByName(newSceneName);
            Scene managerScene = SceneManager.GetSceneByName(sceneTypeToIndex[SceneType.ManagerScene]);
            SceneManager.SetActiveScene(managerScene);
            Time.timeScale = 0f;
            
            
            yield return WaitForAnimationEnd(opts.animator, opts.stateName);

            SceneManager.SetActiveScene(newScene);
            Time.timeScale = 1f;
            Destroy(opts.animator.gameObject);
        }

        private IEnumerator ProcessSceneLoadingWithFade(string newSceneName)
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

        private static IEnumerator WaitForAnimationEnd(Animator animator, string stateName)
        {
            while (!animator.GetCurrentAnimatorStateInfo(0).IsName(stateName))
                yield return null;

            while (animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1.0f)
                yield return null;
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