using _SLIME.BaseScripts;
using UnityEngine;
using DG.Tweening;

namespace _SLIME.Boss
{
    public class FloatingMagic : ProjectMonoBehavior
    {
        public float floatDistance;
        public float duration;

        private Tween _phaseTween;
        private float _startY;
        private float _startX;
        private float _startZ;
        private float _lastDuration;
        private float _lastFloatDistance;

        private void Start()
        {
            var pos = transform.localPosition;
            _startX = pos.x;
            _startY = pos.y;
            _startZ = pos.z;
            _lastDuration = duration;
            _lastFloatDistance = floatDistance;
            StartPhaseTween();
        }

        private void Update()
        {
            if (Mathf.Abs(_lastFloatDistance - floatDistance) > 0.001f || Mathf.Abs(_lastDuration - duration) > 0.001f)
            {
                _lastDuration = duration;
                _lastFloatDistance = floatDistance;
                StartPhaseTween();
            }
        }

        private void OnDisable()
        {
            _phaseTween?.Kill();
        }

        private void StartPhaseTween()
        {
            _phaseTween?.Kill();
            float phase = 0f;
            _phaseTween = DOTween.To(() => phase, x => phase = x, 1f, duration)
                .SetLoops(-1, LoopType.Restart)
                .SetEase(Ease.Linear)
                .OnStepComplete(() => phase = 0f)
                .OnUpdate(() =>
                {
                    float half = floatDistance * 0.5f;
                    float y = _startY + half * Mathf.Sin(phase * Mathf.PI * 2f);
                    transform.localPosition = new Vector3(_startX, y, _startZ);
                });
        }
    }
}