using System;
using _SLIME.BaseScripts;
using _SLIME.Boss;
using _SLIME.UI;
using UnityEngine;
using EventType = _SLIME.UI.EventType;

namespace _SLIME.LittleBoss
{
    [Serializable]
    public struct LittleBossHealthRef
    {
        public Animator Animator;
    }
    
    [Serializable]
    public struct LittleBossHealthSet
    {
        public float health;
    }
    public class LittleBossHealth: ProjectMonoBehavior,IHealth
    {
        private static readonly int Death = Animator.StringToHash("Death");
        private static readonly int Hurt = Animator.StringToHash("Hurt");
        [SerializeField] private LittleBossHealthRef healthRef;
        [SerializeField] private BaseBossConfigurations config;
        private float _health;
        private LittleBossHealthSet HealthSet => config.LittleBossHealth;

        public void Start() { _health = HealthSet.health;  }
        public void TakeDamage(float damage)
        {
            _health -= damage;
            PopupEventsRenderer.OnRenderPointsAbove(new RenderEvent
            {
                eventType = EventType.BossHealth,
                value = -damage,
                fatherTransform = null,
                position = transform.position,
                OnFinish = null
            });
            healthRef.Animator.SetTrigger(_health <= 0 ? Death : Hurt);
        }
    }
}