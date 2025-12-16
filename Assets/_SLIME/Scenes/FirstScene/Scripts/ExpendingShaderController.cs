using UnityEngine;
using UnityEngine.SceneManagement;

public class ExpandingShaderController : MonoBehaviour
{
    [Header("Settings")]
    public float targetRadius = 2.5f; // How big the circle gets (to cover full screen)
    public float duration = 2f;
    public string nextSceneName = "Halel";

    private Renderer _renderer;
    private MaterialPropertyBlock _propBlock;
    private float _timer;
    
    // State Flags
    private bool _isCoveringScreen = true; // Phase 1: Growing to cover screen
    private bool _hasTriggeredLoad = false;

    private static readonly int RadiusID = Shader.PropertyToID("_Radius");

    void Awake()
    {
        _renderer = GetComponent<Renderer>();
        _propBlock = new MaterialPropertyBlock();
        
        // Keep this object alive so it can finish the animation in the next scene
        DontDestroyOnLoad(this.gameObject);
    }

    void Update()
    {
        _timer += Time.deltaTime;
        float progress = Mathf.Clamp01(_timer / duration);

        if (_isCoveringScreen)
        {
            // PHASE 1: EXPAND (0 -> Max)
            // Grow the circle until it covers the screen
            float currentRadius = Mathf.Lerp(0f, targetRadius, progress);
            UpdateShader(currentRadius);

            // Once fully covered, load the scene
            if (progress >= 1f && !_hasTriggeredLoad)
            {
                _hasTriggeredLoad = true;
                LoadSceneByName();
            }
        }
        else
        {
            // PHASE 2: SHRINK (Max -> 0)
            // Shrink the circle away to reveal the new level
            float currentRadius = Mathf.Lerp(targetRadius, 0f, progress);
            UpdateShader(currentRadius);

            // Once fully gone, destroy this transition object
            if (progress >= 1f)
            {
                Destroy(gameObject);
            }
        }
    }

    void UpdateShader(float radius)
    {
        _renderer.GetPropertyBlock(_propBlock);
        _propBlock.SetFloat(RadiusID, radius);
        _renderer.SetPropertyBlock(_propBlock);
    }

    void LoadSceneByName()
    {
        if (!string.IsNullOrEmpty(nextSceneName))
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
            SceneManager.LoadScene(nextSceneName);
        }
        else
        {
            Debug.LogError("Please enter a Scene Name in the Inspector!");
        }
    }

    // This runs automatically when the new scene finishes loading
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;

        // Reset for Phase 2
        _timer = 0f;
        _isCoveringScreen = false; // Switch mode to "Shrink"
    }
}