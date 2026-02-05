using _SLIME.BaseScripts;
using _SLIME.GameLoop;
using UnityEngine;

namespace _SLIME.Scenes.Ending.Scripts
{
    public class EndingActivator: ProjectMonoBehavior
    {
        [SerializeField] private GameObject end;
        
        private bool _isSlimeWon;
        private bool _isTunnel;
        
        private void OnEnable()
        {
            GameEvents.TunnelPhaseStarted += MarkTunnel;
            GameEvents.SlimeWon += MarkSlimeWon;
            GameEvents.WaterAttackEnded += ActivateEnd;
        }
        
        private void OnDisable()
        {
            GameEvents.TunnelPhaseStarted -= MarkTunnel;
            GameEvents.SlimeWon -= MarkSlimeWon;
            GameEvents.WaterAttackEnded -= ActivateEnd;
        }

        private void MarkSlimeWon()
        {
            _isSlimeWon = true;
        }
        private void MarkTunnel()
        {
            _isTunnel = true;
        }

        private void ActivateEnd()
        {
            if (_isTunnel && _isSlimeWon)
                end.SetActive(true);
        }
    }
}