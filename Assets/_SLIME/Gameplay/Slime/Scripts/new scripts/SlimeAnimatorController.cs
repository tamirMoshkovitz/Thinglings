using UnityEngine;

namespace _SLIME.Gameplay.Slime.Scripts.new_scripts
{
    public class SlimeAnimatorController
    {
        private Animator _animator;

        public SlimeAnimatorController(Animator animator)
        {
            _animator = animator;
        }

        public void OnEnable()
        {
            SlimeEvents.SlimeTears += OnSlimeTears;
        }

        public void OnDisable()
        {
            SlimeEvents.SlimeTears -= OnSlimeTears;
        }

        private void OnSlimeTears()
        {
            // _animator?.SetBool();
        }
    }
}