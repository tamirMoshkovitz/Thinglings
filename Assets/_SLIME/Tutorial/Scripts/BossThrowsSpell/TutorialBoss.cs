using System;
using System.Collections;
using System.Collections.Generic;
using _SLIME.BaseScripts;
using _SLIME.Core.Audio.FMOD.Scripts;
using _SLIME.GameLoop;
using FMODUnity;
using UnityEngine;

namespace _SLIME.Tutorial
{
    [System.Serializable]
    public struct TutorialBossFlashSettings
    {
        public Color flashColor;
        public string shaderPropertyName;
        public float flashDuration;
        public float maxIntensity;
    }

    public class TutorialBoss : ProjectMonoBehavior, IHealth
    {
        public static event Action BossHit;
        [SerializeField] private TutorialScriptable tutorialScriptable;
        [SerializeField] private List<GameObject> bossRenderersToFlash = new List<GameObject>();
        [SerializeField] private EventReference hitSFX;

        private TutorialBossFlashSettings _flashSettings;
        private List<Renderer> _renderers = new List<Renderer>();
        private int _flashPropertyID;
        private Coroutine _flashRoutine;

        private void Awake()
        {
            _flashSettings = tutorialScriptable.TutorialBossFlashSettings;
            _flashPropertyID = Shader.PropertyToID(_flashSettings.shaderPropertyName);
            
            foreach (var parent in bossRenderersToFlash)
            {
                _renderers.AddRange(parent.GetComponentsInChildren<Renderer>(true));
            }
        }

        private void OnEnable()
        {
            GameEvents.FmodPhaseOne?.Invoke();
        }

        private void OnDisable()
        {
            ResetFlash();
        }

        public void TakeDamage(float damage = 0F)
        {
            SFXPlayer.Play(hitSFX);
            TriggerFlash();
            BossHit?.Invoke();
        }

        private void TriggerFlash()
        {
            if (_flashRoutine != null) StopCoroutine(_flashRoutine);
            
            ResetFlash();
            _flashRoutine = StartCoroutine(FlashRoutine());
        }

        private IEnumerator FlashRoutine()
        {
            float timer = 0f;

            while (timer < _flashSettings.flashDuration)
            {
                timer += Time.deltaTime;
                float currentVal = Mathf.Lerp(_flashSettings.maxIntensity, 0f, timer / _flashSettings.flashDuration);
                
                Color finalColor = _flashSettings.flashColor * currentVal;

                foreach (var r in _renderers)
                {
                    r.material.SetVector(_flashPropertyID, finalColor);
                }

                yield return null;
            }

            foreach (var r in _renderers)
            {
                r.material.SetVector(_flashPropertyID, Vector3.zero);
            }

            _flashRoutine = null;
        }

        private void ResetFlash()
        {
            foreach (var r in _renderers)
            {
                r.material.SetVector(_flashPropertyID, Vector3.zero);
            }
        }
    }
}
