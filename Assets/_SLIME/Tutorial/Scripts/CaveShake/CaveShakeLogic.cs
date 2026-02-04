using System.Collections;
using _SLIME.Envierment.Earthquake.Scriptables;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Serialization;

namespace _SLIME.Tutorial
{
    [System.Serializable]
    public struct CaveShakeStateDeps
    {
        public Animator iciclesAnimator;
        public Camera camera;
    }
    
    [System.Serializable]
    public struct CaveShakeStateSet
    {
         public EarthquakeUtil earthquakeUtil;
    }
    
    public class CaveShakeLogic : ITutorialStateLogic
    {
        private static readonly int Stalactites = Animator.StringToHash("Broken Stalactites");
        private readonly CaveShakeStateDeps _caveShakeStateDeps;
        private readonly CaveShakeStateSet _caveShakeStateSet;
        
        public CaveShakeLogic(CaveShakeStateDeps caveShakeStateDeps,
            CaveShakeStateSet caveShakeStateSet)
        {
            _caveShakeStateDeps = caveShakeStateDeps;
            _caveShakeStateSet = caveShakeStateSet;
        }
        
        public void OnDisable()
        {
            // Cleanup if needed
        }
        
        public IEnumerator Start()
        {
            yield return _caveShakeStateSet.earthquakeUtil.EarthquakeCoroutine(_caveShakeStateDeps.camera, _caveShakeStateDeps.iciclesAnimator, Stalactites);
        }
    }
}
