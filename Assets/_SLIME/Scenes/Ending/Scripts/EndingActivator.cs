using _SLIME.BaseScripts;
using _SLIME.GameLoop;
using UnityEngine;

namespace _SLIME.Scenes.Ending.Scripts
{
    public class EndingActivator: ProjectMonoBehavior
    {
        [SerializeField] private GameObject end;
        
        private bool _isSlimeWon;
        
        private void OnEnable()
        {
            GameEvents.SlimeWon += MarkSlimeWon;
            GameEvents.WaterAttackEnded += ActivateEnd;
        }
        
        private void OnDisable()
        {
            GameEvents.SlimeWon -= MarkSlimeWon;
            GameEvents.WaterAttackEnded -= ActivateEnd;
        }

        private void MarkSlimeWon()
        {
            _isSlimeWon = true;
        }

        private void ActivateEnd()
        {
            end.SetActive(true);
        }
    }
}