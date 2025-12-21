
using DG.Tweening;
using UnityEngine;

namespace _SLIME.Slime
{
    public class SlimeStretchCameraShake : ISlimeBehaviorComponent
    {
        private readonly SlimeStretchCameraShakeConfiguration _configuration;
        private SlimeData _slimeData;
        private readonly Camera _mainCamera;
        private float _shakeTimer;

        public SlimeStretchCameraShake(SlimeStretchCameraShakeConfiguration configuration, SlimeData slimeData, Camera mainCamera)
        {
            _configuration = configuration;
            _slimeData = slimeData;
            _mainCamera = mainCamera;
        }

        public void OnDestroy() { }

        public void UpdateStretch()
        {
            _shakeTimer += Time.deltaTime;
            if (!(_shakeTimer >= _configuration.ShakeUpdateFrequency)) return;
            
            float currentStrength = _configuration.StretchShakeStrength * Mathf.Pow(_slimeData.Distance, 2f);
            _mainCamera.transform.DOShakePosition(_configuration.ShakeUpdateFrequency, currentStrength);
            _shakeTimer = 0f;
            
        }

        public void OnSlimeTears()
        {
            _mainCamera.transform.DOShakePosition(_configuration.ShakeDuration, _configuration.TearShakeStrength);
        }

        public void OnSlimeConnected() { }

        public void OnTearFinished() { }

        public void OnPauseGame() { }

        public void OnResumeGame() { }
    }
}