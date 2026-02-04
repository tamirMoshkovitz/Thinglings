using System;
using System.Collections;
using _SLIME.BaseScripts;
using _SLIME.GameLoop;
using UnityEngine;

namespace _SLIME.Envierment.Earthquake.Scriptables
{
    public class StalactitesTunnelAnimation: ProjectMonoBehavior
    {
        [SerializeField] private float upscaleLevel;
        [SerializeField] private float lerpTime;
        private bool _isTunnelStarted;
        private Vector3 _originalScale;

        private void Awake()
        {
            _originalScale = transform.localScale;
        }

        private void OnEnable()
        {
            GameEvents.TunnelPhaseStarted += OnTunnelStarted;
            GameEvents.WaterAttackStarted += OnTunnelEnded;
        }

        private void OnDisable()
        {
            GameEvents.TunnelPhaseStarted -= OnTunnelStarted;
            GameEvents.WaterAttackStarted -= OnTunnelEnded;
        }

        private void OnTunnelStarted()
        {
            _isTunnelStarted = true;
            
            StartCoroutine(LerpScale(1f, upscaleLevel));
        }

        private void OnTunnelEnded()
        {
            if (!_isTunnelStarted) return;

            float currentScale = transform.localScale.x / _originalScale.x;
            StartCoroutine(LerpScale(currentScale, 1f));
        }

        private IEnumerator LerpScale(float startScale, float targetScale)
        {
            float timer = 0f;
            while (timer < lerpTime)
            {
                float scale = Mathf.Lerp(startScale, targetScale, timer / lerpTime);
                transform.localScale = _originalScale * scale;
                timer += Time.deltaTime;
                yield return null;
            }
            transform.localScale = _originalScale * targetScale;
        }
    }
}