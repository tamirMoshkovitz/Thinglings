using UnityEngine;
using System.Collections;
using _SLIME.BaseScripts;
using _SLIME.GameLoop;


namespace _SLIME.Boss
{
    public class HitFlashEffect : ProjectMonoBehavior
    {
        [Header("Settings")]
        [Tooltip("The Renderer to flash. If empty, it grabs the one on this object.")]
        [SerializeField]
        private Renderer _targetRenderer;

        [Tooltip("The exact name of the property in your Shader Graph (e.g. '_Flash', 'Flash', or '_HitColor')")]
        [SerializeField]
        private string _shaderPropertyName = "_Flash";

        [SerializeField] private float _flashDuration = 0.2f;
        [SerializeField] private float _maxIntensity = 5f;

        private int _flashPropertyID;
        private Coroutine _flashRoutine;
        private Material _materialInstance;

        private void Awake()
        {
            // Auto-grab renderer if not assigned
            if (_targetRenderer == null)
                _targetRenderer = GetComponent<Renderer>();

            // Cache the shader ID for performance (much faster than using strings)
            _flashPropertyID = Shader.PropertyToID(_shaderPropertyName);

            // We act on the material instance so we don't change the asset file
            if (_targetRenderer != null)
                _materialInstance = _targetRenderer.material;
        }

        private void OnEnable()
        {
            GameEvents.EnemyGotBricked += TriggerFlash;
        }

        private void OnDisable()
        {
            GameEvents.EnemyGotBricked -= TriggerFlash;
        }

        public void TriggerFlash()
        {
            if (_targetRenderer == null || _materialInstance == null) return;

            // Stop any currently running flash so they don't fight
            if (_flashRoutine != null) StopCoroutine(_flashRoutine);

            _flashRoutine = StartCoroutine(FlashRoutine());
        }

        private IEnumerator FlashRoutine()
        {
            float timer = 0f;

            while (timer < _flashDuration)
            {
                timer += Time.deltaTime;

                // Fade from Max Intensity down to 0
                float currentVal = Mathf.Lerp(_maxIntensity, 0f, timer / _flashDuration);

                // Construct the vector (Assuming Red channel controls the flash)
                Vector3 flashVector = new Vector3(currentVal, 0, 0);

                _materialInstance.SetVector(_flashPropertyID, flashVector);

                yield return null;
            }

            // Ensure we finish cleanly at exactly 0
            _materialInstance.SetVector(_flashPropertyID, Vector3.zero);
            _flashRoutine = null;
        }
    }

}