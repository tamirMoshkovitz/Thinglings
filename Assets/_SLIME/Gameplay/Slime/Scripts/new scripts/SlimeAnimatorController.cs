using UnityEngine;

namespace _SLIME.Gameplay.Slime.Scripts.new_scripts
{
    public class SlimeAnimatorController
    {
        private static readonly int IsMoving = Animator.StringToHash("IsMoving");
        private static readonly int IsStraining = Animator.StringToHash("isStraining");
        private static readonly int Heal = Animator.StringToHash("Heal");
        private static readonly int Hit = Animator.StringToHash("Hit");
        private Animator _animator;

        public SlimeAnimatorController(Animator animator)
        {
            _animator = animator;
        }

        public void Update(bool isMoving, bool isStraining)
        {
            _animator.SetBool(IsMoving, isMoving);
            _animator.SetBool(IsStraining, isStraining);
        }

        public void SetHeal()
        {
            _animator.SetTrigger(Heal);
        }

        public void SetHit()
        {
            _animator.SetTrigger(Hit);
            _animator.SetBool(IsStraining, false);
        }
    }
}