using _SLIME.BaseScripts;
using UnityEngine;

namespace _SLIME.Tutorial
{
    public class OptionsRock: ProjectMonoBehavior
    {
        private static readonly int Out = Animator.StringToHash("Out");
        private static readonly int Inside = Animator.StringToHash("Inside");
        [SerializeField] private Animator optionsAnimator;
        public enum OptionsState
        {
            NO,
            YES
        }
        
        private OptionsState _state = OptionsState.NO;
        public void optionsPressed()
        {
            if (_state == OptionsState.NO)
            {
                optionsAnimator.SetTrigger(Out);
                _state = OptionsState.YES;
            }
            else
            {
                optionsAnimator.SetTrigger(Inside);
                _state = OptionsState.NO;
            }
        }
    }
}