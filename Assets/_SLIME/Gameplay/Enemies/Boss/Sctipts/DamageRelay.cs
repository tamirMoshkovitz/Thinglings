using _SLIME.BaseScripts;
using _SLIME.UI;

namespace _SLIME.Boss
{
    public class DamageRelay : ProjectMonoBehavior
    {
        public BossController bossBrain;

        public void TakeDamage(float amount)
        {
            bossBrain.TakeDamage(amount);
            PopupEventsRenderer.OnRenderPointsAbove(new RenderEvent
            {
                eventType = EventType.BossHealth,
                value = -amount,
                fatherTransform = null,
                position = transform.position,
                OnFinish = null
            });
        }
    }
}