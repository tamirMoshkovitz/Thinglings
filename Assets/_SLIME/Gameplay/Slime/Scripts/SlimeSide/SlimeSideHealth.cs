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
            public SlimeData Data;
            
            public SlimeSideHealthFormat(SlimeSide parent, float maxHealth, GameObject playerHitPoint, 
                SlimeAnimatorController animatorController, SlimeData data)
            {
                Parent = parent;
                MaxHealth = maxHealth;
                PlayerHitPoint = playerHitPoint;
                AnimatorController = animatorController;
                Data = data;
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
        private SlimeData _data;
        #endregion
        
        public SlimeSideHealth(SlimeSideHealthFormat format)
        {
            _parent = format.Parent;
            _maxHealth = format.MaxHealth;
            _playerHitPoint = format.PlayerHitPoint;
            CurrentHealth = _maxHealth;
            _animatorController = format.AnimatorController;
            _data = format.Data;
            var hit = _parent.Transform.GetComponent<SlimeSideHitCollider>();
            if (hit != null) hit.Init(this);
        }

        public void OnEnable()
        {
            SlimeEvents.SlimeConnected += OnSlimeConnected;
            SlimeEvents.SlimeTears += OnSlimeTear;
        }

        public void OnDisable()
        {
            SlimeEvents.SlimeConnected -= OnSlimeConnected;
            SlimeEvents.SlimeTears -= OnSlimeTear;
        }

        public void Update() { }

        public void TakeDamage(float damage)
        {
            IsDead = true;
            _data.OneSlimeDead = true;
            SlimeEvents.SlimeGetHit();
            // TODO(Elad): I'm not sure for what this is required 
            // if (IsDead) return;
            //
            // CurrentHealth -= damage;
            // if (CurrentHealth <= 0)
            // {
            //     CurrentHealth = 0;
            //     IsDead = true;
            // } 
        }

        public void Resurrect()
        {
            IsDead = false;
            _data.OneSlimeDead = false;
        }

        private void OnSlimeConnected()
        {
            Resurrect();
            
        }

        private void OnSlimeTear() { }

       
    }
}