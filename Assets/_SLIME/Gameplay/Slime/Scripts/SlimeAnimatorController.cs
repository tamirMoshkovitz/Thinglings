using UnityEngine;

namespace _SLIME.Slime
{
    public class SlimeAnimatorController
    {
        private static readonly int IsMoving = Animator.StringToHash("IsMoving");
        private static readonly int IsStraining = Animator.StringToHash("isStraining");
        private static readonly int Heal = Animator.StringToHash("Heal");
        private static readonly int Hit = Animator.StringToHash("Hit");
        private static readonly int StartChange = Animator.StringToHash("Start Change");
        private static readonly int EndChange = Animator.StringToHash("End Change");
        private Animator _animator;
        private readonly Renderer _renderer;

        public SlimeAnimatorController(Animator animator,
            Renderer renderer)
        {
            _animator = animator;
            _renderer = renderer;
        }
        
        public Animator Animator => _animator;

        public void Update(bool isMoving, bool isStraining)
        {
            _animator.SetBool(IsMoving, isMoving);
            _animator.SetBool(IsStraining, isStraining);
        }

        public void SetHeal()
        {
            _animator.ResetTrigger(Hit);
            _animator.SetTrigger(Heal);
        }

        public void SetHit()
        {
            _renderer.material.color = Color.black; 
            // TODO: the upper line will be removed when we have animation of death
            _animator.SetTrigger(Hit);
            _animator.SetBool(IsStraining, false);
        }

        public void SetStartChange()
        {
            _animator.SetTrigger(StartChange);
        }
        
        public void SetEndChange()
        {
            _animator.SetTrigger(EndChange);
        }
    }
}