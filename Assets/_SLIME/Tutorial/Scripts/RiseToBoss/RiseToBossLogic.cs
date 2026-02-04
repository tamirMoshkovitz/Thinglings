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
            float lastBgY = bg.position.y;
            float targetY = lastBgY - _riseToBossStateDeps.maxCameraPosition.position.y;
            
            var rb1 = _riseToBossStateDeps.Slime1 != null ? _riseToBossStateDeps.Slime1.GetComponent<Rigidbody2D>() : null;
            var rb2 = _riseToBossStateDeps.Slime2 != null ? _riseToBossStateDeps.Slime2.GetComponent<Rigidbody2D>() : null;
            
            _backgroundMoveTween = bg
                .DOMoveY(targetY, _riseToBossStateSet.moveSpeed)
                .SetEase(_riseToBossStateSet.moveEase);
            
            while (_backgroundMoveTween != null && _backgroundMoveTween.IsActive() && _backgroundMoveTween.IsPlaying())
            {
                yield return new WaitForEndOfFrame();
                
                float currentBgY = bg.position.y;
                float deltaThisFrame = lastBgY - currentBgY;
                lastBgY = currentBgY;
                
                if (rb1 != null)
                {
                    var pos = rb1.position;
                    rb1.position = new Vector2(pos.x, pos.y - deltaThisFrame);
                }
                else if (_riseToBossStateDeps.Slime1 != null)
                {
                    var p = _riseToBossStateDeps.Slime1.transform.position;
                    _riseToBossStateDeps.Slime1.transform.position = new Vector3(p.x, p.y - deltaThisFrame, p.z);
                }
                if (rb2 != null)
                {
                    var pos = rb2.position;
                    rb2.position = new Vector2(pos.x, pos.y - deltaThisFrame);
                }
                else if (_riseToBossStateDeps.Slime2 != null)
                {
                    var p = _riseToBossStateDeps.Slime2.transform.position;
                    _riseToBossStateDeps.Slime2.transform.position = new Vector3(p.x, p.y - deltaThisFrame, p.z);
                }
            }
        }
    }
}