using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using _SLIME.BaseScripts;
using _SLIME.GameLoop;
// using _SLIME.Boss.States; 

namespace _SLIME.Boss
{
    public class HitFlashEffect : ProjectMonoBehavior
    {
        [Header("Target Groups")]
        [Tooltip("Assign the parent GameObject for the Boss's 'Close' form here.")]
        [SerializeField] private List<GameObject> _closeStateParents = new List<GameObject>();

        [Tooltip("Assign the parent GameObject for the Boss's 'Far' form here.")]
        [SerializeField] private List<GameObject> _farStateParents = new List<GameObject>();

        [Header("Effect Settings")]
        [Tooltip("The color of the flash. Set to White for a standard brightness flash.")]
        [SerializeField] private Color _flashColor = Color.white; // Added this to control color
        [SerializeField] private string _shaderPropertyName = "_Flash";
        [SerializeField] private float _flashDuration = 0.2f;
        [SerializeField] private float _maxIntensity = 5f;

        // We store Renderers now, not Materials
        private List<Renderer> _closeRenderers = new List<Renderer>();
        private List<Renderer> _farRenderers = new List<Renderer>();

        private int _flashPropertyID;
        private Coroutine _flashRoutine;

        private void Awake()
        {
            _flashPropertyID = Shader.PropertyToID(_shaderPropertyName);
            
            // Populate the renderer lists once at startup
            GetRenderersFromParents(_closeStateParents, _closeRenderers);
            GetRenderersFromParents(_farStateParents, _farRenderers);
        }

        private void OnEnable()
        {
            GameEvents.EnemyGotBricked += TriggerFlash;
        }

        private void OnDisable()
        {
            GameEvents.EnemyGotBricked -= TriggerFlash;
            ResetFlash();
        }

        private void GetRenderersFromParents(List<GameObject> parents, List<Renderer> targetList)
        {
            foreach (var parent in parents)
            {
                if (parent != null)
                {
                    targetList.AddRange(parent.GetComponentsInChildren<Renderer>(true));
                }
            }
        }

        public void TriggerFlash()
        {
            if (_flashRoutine != null) StopCoroutine(_flashRoutine);
            
            ResetFlash();
            
            List<Renderer> activeRenderers = null;

            if (BossBrain._bossState is BossStates.CloseState)
            {
                activeRenderers = _closeRenderers;
            }
            else if (BossBrain._bossState is BossStates.FarState)
            {
                activeRenderers = _farRenderers;
            }

            if (activeRenderers != null && activeRenderers.Count > 0)
            {
                _flashRoutine = StartCoroutine(FlashRoutine(activeRenderers));
            }
        }

        private IEnumerator FlashRoutine(List<Renderer> targets)
        {
            float timer = 0f;

            while (timer < _flashDuration)
            {
                timer += Time.deltaTime;
                float currentVal = Mathf.Lerp(_maxIntensity, 0f, timer / _flashDuration);
                
                Color finalColor = _flashColor * currentVal; 

                foreach (var r in targets)
                {
                    if (r)
                    {
                        r.material.SetVector(_flashPropertyID, finalColor);
                    }
                }

                yield return null;
            }

            // Clean up: Reset to 0 (Black/No emission)
            foreach (var r in targets)
            {
                if (r)
                {
                    r.material.SetVector(_flashPropertyID, Vector3.zero);
                }
            }

            _flashRoutine = null;
        }

        private void ResetFlash()
        {
            ResetRenderers(_closeRenderers);
            ResetRenderers(_farRenderers);
        }

        private void ResetRenderers(List<Renderer> list)
        {
            foreach (var r in list)
            {
                if (r != null)
                {
                    r.material.SetVector(_flashPropertyID, Vector3.zero);
                }
            }
        }
    }
}