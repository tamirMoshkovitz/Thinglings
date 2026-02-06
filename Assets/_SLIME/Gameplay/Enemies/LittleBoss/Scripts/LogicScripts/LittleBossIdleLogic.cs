using System;
using UnityEngine;

namespace _SLIME.LittleBoss
{
    [Serializable]
    public struct LittleBossIdleSettings
    {
        public float timeUntilAttack;
    }

    [Serializable]
    public struct LittleBossIdleRef
    {
        public Animator animator;
    }

    public class LittleBossIdleLogic : LittleBossBaseLogic
    {
        private static readonly int Attack = Animator.StringToHash("Attack");
        public LittleBossIdleSettings Set;
        private readonly LittleBossIdleRef _ref;

        private float _timer;
        private bool _active;

        public LittleBossIdleLogic(LittleBossIdleSettings set, LittleBossIdleRef reference)
        {
            Set = set;
            _ref = reference;
        }

        public void StartLogic()
        {
            _timer = 0f;
            _active = true;
        }

        public void EndLogic()
        {
            _active = false;
        }

        public void UpdateLogic()
        {
            if (!_active) return;

            _timer += Time.deltaTime;
            if (_timer >= Set.timeUntilAttack)
            {
                _ref.animator.SetTrigger(Attack);
                _active = false;
            }
        }
    }
}