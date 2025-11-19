using System;
using DG.Tweening;
using Player.Interfaces;
using UnityEngine;

namespace Player
{
    [Serializable]
    public class SlimeStretchCameraShake : ISlimeBehaviorComponent
    {
        [SerializeField] Camera mainCamera;
        [SerializeField] float shakeDuration = 0.5f;
        [SerializeField] float tearShakeStrength = 0.5f;
        [SerializeField] float stretchShakeStrength = 0.05f;
        [SerializeField] float shakeUpdateFrequency = 0.1f;
        
        private SlimeData _slimeData;
        private float _shakeTimer;
        
        public ISlimeBehaviorComponent Awake(SlimeData slimeData)
        {
            _slimeData = slimeData;
            return this;
        }

        public void OnDestroy() { }

        public void UpdateStretch()
        {
            _shakeTimer += Time.deltaTime;
            if (!(_shakeTimer >= shakeUpdateFrequency)) return;
            
            float currentStrength = stretchShakeStrength * Mathf.Pow(_slimeData.Distance, 2f);
            mainCamera.transform.DOShakePosition(shakeUpdateFrequency, currentStrength);
            _shakeTimer = 0f;
            
        }

        public void OnSlimeTears()
        {
            mainCamera.transform.DOShakePosition(shakeDuration, tearShakeStrength);
        }

        public void OnTearFinished() { }

        public void OnPauseGame() { }

        public void OnResumeGame() { }
    }
}