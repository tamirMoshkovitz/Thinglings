using _SLIME.Core.Audio.FMOD.Scripts;
using FMODUnity;
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
            public EventReference DeadSlimeSFX;
            
            public SlimeSideHealthFormat(SlimeSide parent, float maxHealth, GameObject playerHitPoint, 
                SlimeAnimatorController animatorController, SlimeData data, EventReference deadSlimeSFX)
            {
                Parent = parent;
                MaxHealth = maxHealth;
                PlayerHitPoint = playerHitPoint;
                AnimatorController = animatorController;
                Data = data;
                DeadSlimeSFX = deadSlimeSFX;
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
        private EventReference _deadSlimeSFX;
        #endregion
        
        public SlimeSideHealth(SlimeSideHealthFormat format)
        {
            _parent = format.Parent;
            _maxHealth = format.MaxHealth;
            _playerHitPoint = format.PlayerHitPoint;
            CurrentHealth = _maxHealth;
            _animatorController = format.AnimatorController;
            _data = format.Data;
            _deadSlimeSFX = format.DeadSlimeSFX;
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

        public void Update()
        {
            if (IsDead) _animatorController?.SetHit();
        }

        public void TakeDamage(float damage)
        {
            if (IsDead) return;
            
            SFXPlayer.Play(_deadSlimeSFX);
            IsDead = true;
            _data.OneSlimeDead = true;
            SlimeEvents.SlimeGetHit?.Invoke();
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