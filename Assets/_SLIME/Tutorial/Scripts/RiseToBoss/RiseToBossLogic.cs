using System.Collections;
using _SLIME.Slime;
using DG.Tweening;
using UnityEngine;

namespace _SLIME.Tutorial
{
    [System.Serializable]
    public struct RiseToBossStateDeps
    {
        public GameObject Slime1;
        public GameObject Slime2;
        public Animator Arrow1;
        public Animator Arrow2;
        public Camera mainCamera;
        public Transform maxCameraPosition;
        public Transform Backgorund;
    }
    
    [System.Serializable]
    public struct RiseToBossStateSet
    {
        [Range(1, 100)]
        public int topThresholdPercent;
        public float moveSpeed;
        public Ease moveEase;
        public float cameraNotMovingTimeout;
    }
    
    public class RiseToBossLogic : ITutorialStateLogic
    {
        private RiseToBossStateDeps _riseToBossStateDeps;
        private RiseToBossStateSet _riseToBossStateSet;
        private Tween _backgroundMoveTween;
        
        public RiseToBossLogic(RiseToBossStateDeps riseToBossStateDeps,
            RiseToBossStateSet riseToBossStateSet)
        {
            _riseToBossStateDeps = riseToBossStateDeps;
            _riseToBossStateSet = riseToBossStateSet;
        }
        
        public void OnDisable()
        {
            // Cleanup if needed
        }
        
        public IEnumerator Start()
        {
            SlimeEvents.RemoveCameraShake();
            yield return WaitForSlimesAtTop();
            TriggerArrowsOut();
            yield return MoveBackgroundDown();
            SlimeEvents.AddCameraShake();
        }
        
        private IEnumerator WaitForSlimesAtTop()
        {
            float lastCameraY = _riseToBossStateDeps.mainCamera.transform.position.y;
            float timeSinceLastMovement = 0f;
            bool arrowsTriggered = false;
            
            while (!AreSlimesAtTop())
            {
                yield return null;
                 
                timeSinceLastMovement += Time.deltaTime;
                
                if (!arrowsTriggered && timeSinceLastMovement >= _riseToBossStateSet.cameraNotMovingTimeout)
                {
                    TriggerArrowsIn();
                    arrowsTriggered = true;
                }
            }
        }
        
        private void TriggerArrowsIn()
        {
            _riseToBossStateDeps.Arrow1.SetTrigger("arrow in");
            _riseToBossStateDeps.Arrow2.SetTrigger("arrow in");
        }
        
        private void TriggerArrowsOut()
        {
            _riseToBossStateDeps.Arrow1.SetTrigger("arrow out");
            _riseToBossStateDeps.Arrow2.SetTrigger("arrow out");
        }
        
        private bool AreSlimesAtTop()
        {
            Camera cam = _riseToBossStateDeps.mainCamera;
            float cameraCenter = cam.transform.position.y;
            float orthographicSize = cam.orthographicSize;
            float thresholdPercentage = _riseToBossStateSet.topThresholdPercent / 100f;
            float threshold = cameraCenter + orthographicSize * (1f - thresholdPercentage);
            
            float slime1Y = _riseToBossStateDeps.Slime1.transform.position.y;
            float slime2Y = _riseToBossStateDeps.Slime2.transform.position.y;
            
            return slime1Y >= threshold && slime2Y >= threshold;
        }
        
        private IEnumerator MoveBackgroundDown()
        {
            Transform bg = _riseToBossStateDeps.Backgorund;
            float targetY = bg.position.y - _riseToBossStateDeps.maxCameraPosition.position.y;
            
            _backgroundMoveTween = bg
                .DOMoveY(targetY, _riseToBossStateSet.moveSpeed)
                .SetEase(_riseToBossStateSet.moveEase);
            
            yield return _backgroundMoveTween.WaitForCompletion();
        }
    }
}