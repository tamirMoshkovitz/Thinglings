using _SLIME.BaseScripts;
using UnityEngine;
using DG.Tweening;

namespace _SLIME.Boss
{
    public class FloatingMagic : ProjectMonoBehavior
    {
        public float floatDistance;
        public float duration;
        [Tooltip("If true, move in a vertical figure-eight pattern instead of simple up-down.")]
        public bool useFigureEight;

        [Tooltip("How long it takes for the movement to reach full amplitude.")]
        public float easeInDuration = 1.5f; 

        private Tween _phaseTween;
        private Tween _intensityTween; // טווין נפרד לעוצמת התנועה
        
        private float _startY;
        private float _startX;
        private float _startZ;
        
        private float _lastDuration;
        private float _lastFloatDistance;
        private bool _lastUseFigureEight;

        // המשתנה הזה יחזיק ערך בין 0 ל-1 וישמש כמכפיל לתנועה
        private float _currentIntensity = 0f; 

        private void Start()
        {
            var pos = transform.localPosition;
            _startX = pos.x;
            _startY = pos.y;
            _startZ = pos.z;
            
            _lastDuration = duration;
            _lastFloatDistance = floatDistance;
            _lastUseFigureEight = useFigureEight;
            
            StartPhaseTween();
        }

        private void Update()
        {
            if (Mathf.Abs(_lastFloatDistance - floatDistance) > 0.001f ||
                Mathf.Abs(_lastDuration - duration) > 0.001f ||
                _lastUseFigureEight != useFigureEight)
            {
                _lastDuration = duration;
                _lastFloatDistance = floatDistance;
                _lastUseFigureEight = useFigureEight;
                
                StartPhaseTween();
            }
        }

        private void OnDisable()
        {
            _phaseTween?.Kill();
            _intensityTween?.Kill();
        }

        private void StartPhaseTween()
        {
            _phaseTween?.Kill();
            _intensityTween?.Kill();

  
            _currentIntensity = 0f;
            _intensityTween = DOTween.To(() => _currentIntensity, x => _currentIntensity = x, 1f, easeInDuration)
                .SetEase(Ease.OutQuad); 

            float phase = 0f;
            

            _phaseTween = DOTween.To(() => phase, x => phase = x, 1f, duration)
                .SetLoops(-1, LoopType.Restart)
                .SetEase(Ease.Linear)
                .OnStepComplete(() => phase = 0f)
                .OnUpdate(() =>
                {
                    float half = floatDistance * 0.5f * _currentIntensity; 
                    float angle = phase * Mathf.PI * 2f;

                    if (!useFigureEight)
                    {
                        // Simple up-down motion
                        float y = _startY + half * Mathf.Sin(angle);
                        transform.localPosition = new Vector3(_startX, y, _startZ);
                    }
                    else
                    {
                        // Figure-eight pattern
                        float x = _startX + half * Mathf.Sin(angle);
                        float y = _startY + half * 0.5f * Mathf.Sin(2f * angle); 
                        transform.localPosition = new Vector3(x, y, _startZ);
                    }
                });
        }
    }
}