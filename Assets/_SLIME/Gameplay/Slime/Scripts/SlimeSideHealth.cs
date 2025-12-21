using UnityEngine;

namespace _SLIME.Slime
{
    public class SlimeSideHealth // TODO: think about the health logic, what is the main body health, how is the healing is done, etc.
    {
        public struct SlimeSideHealthFormat
        {
            public SlimeSide Parent;
            public float MaxHealth;
            public GameObject PlayerHitPoint;
            public SlimeAnimatorController AnimatorController;
            
            public SlimeSideHealthFormat(SlimeSide parent, float maxHealth, GameObject playerHitPoint, SlimeAnimatorController animatorController)
            {
                Parent = parent;
                MaxHealth = maxHealth;
                PlayerHitPoint = playerHitPoint;
                AnimatorController = animatorController;
            }
        }
        
        #region Properties
        public float CurrentHealth { get; private set; }
        private bool _isDead = false;

        public bool IsDead
        {
            get => _isDead;
            private set
            {
                if (IsDead != value)
                {
                    _isDead = value;
                    if (_isDead)
                    {
                        _animatorController?.SetHit();
                    }
                    else // heal
                    {
                        _animatorController?.SetHeal();
                    }
                }
            }
        }

        #endregion
        
        #region Private Fields
        private SlimeSide _parent;
        private GameObject _playerHitPoint;
        private readonly float _maxHealth;
        private SlimeAnimatorController _animatorController;
        #endregion
        
        public SlimeSideHealth(SlimeSideHealthFormat format)
        {
            _parent = format.Parent;
            _maxHealth = format.MaxHealth;
            _playerHitPoint = format.PlayerHitPoint;
            CurrentHealth = _maxHealth;
            _animatorController = format.AnimatorController;
        }

        public void OnEnable()
        {
            SlimeEvents.SlimeConnected += OnSlimeConnected;
            SlimeEvents.SlimeTears += OnSlimeTear;
            SlimeEvents.SlimeGetHit += OnSlimeGetHit;
        }

        public void OnDisable()
        {
            SlimeEvents.SlimeConnected -= OnSlimeConnected;
            SlimeEvents.SlimeTears -= OnSlimeTear;
            SlimeEvents.SlimeGetHit -= OnSlimeGetHit;
        }

        public void Update() { }

        public void TakeDamage(float damage)
        {
            if (IsDead) return;

            CurrentHealth -= damage;
            if (CurrentHealth <= 0)
            {
                CurrentHealth = 0;
                IsDead = true;
            }
        }

        public void Resurrect()
        {
            IsDead = false;
        }

        private void OnSlimeConnected()
        {
            Resurrect();
        }

        private void OnSlimeTear() { }

        private void OnSlimeGetHit(GameObject hitSide)
        {
            if (hitSide ==_playerHitPoint)
            {
                IsDead = true;
            }
        }
    }
}