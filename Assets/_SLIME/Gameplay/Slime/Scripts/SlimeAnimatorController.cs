using UnityEngine;

namespace _SLIME.Slime
{
    public class SlimeAnimatorController
    {
        private static readonly int IsMoving = Animator.StringToHash("IsMoving");
        private static readonly int IsStraining = Animator.StringToHash("isStraining");
        private static readonly int IsDead = Animator.StringToHash("Is Dead");
        private static readonly int IsChanging = Animator.StringToHash("Is Changing");
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
            _animator.SetBool(IsDead, false);
        }

        public void SetHit()
        {
            // _renderer.material.color = Color.black; 
            // // TODO: the upper line will be removed when we have animation of death
            _animator.SetBool(IsDead, true);
            _animator.SetBool(IsStraining, false);
        }

        public void SetStartChange()
        {
            _animator.SetBool(IsChanging, true);
        }
        
        public void SetEndChange()
        {
            _animator.SetBool(IsChanging, false);
        }
    }
}