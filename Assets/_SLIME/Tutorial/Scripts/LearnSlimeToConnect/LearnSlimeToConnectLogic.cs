using System;
using System.Collections;
using _SLIME.Slime;
using UnityEngine;

namespace _SLIME.Tutorial
{
    [System.Serializable]
    public struct LearnSlimeToConnectStateDeps
    {
    }
    
    [System.Serializable]
    public struct LearnSlimeToConnectStateSet
    {
    }
    
    public class LearnSlimeToConnectLogic : ITutorialStateLogic
    {
        private LearnSlimeToConnectStateDeps _learnSlimeToConnectStateDeps;
        private LearnSlimeToConnectStateSet _learnSlimeToConnectStateSet;
        private bool _slimeConnected;
        
        public LearnSlimeToConnectLogic(LearnSlimeToConnectStateDeps learnSlimeToConnectStateDeps,
            LearnSlimeToConnectStateSet learnSlimeToConnectStateSet)
        {
            _learnSlimeToConnectStateDeps = learnSlimeToConnectStateDeps;
            _learnSlimeToConnectStateSet = learnSlimeToConnectStateSet;
             _slimeConnected = false;
            SlimeEvents.SlimeConnected += OnSlimeConnected;
        }
        
        public void OnDisable()
        {
            SlimeEvents.SlimeConnected -= OnSlimeConnected;
        }
        
        public IEnumerator Start()
        {
            yield return WaitForSlimeConnected();
        }
        
        private void OnSlimeConnected()
        {
            _slimeConnected = true;
        }
        
        private IEnumerator WaitForSlimeConnected()
        {
            while (!_slimeConnected)
            {
                yield return null;
            }
        }
    }
}
