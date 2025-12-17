using System;
using UI;
using UnityEngine;
using EventType = UI.EventType;

public class DamageRelay : MonoBehaviour
{
    public BossController bossBrain; 
    
    public void TakeDamage(float amount)
    {
        bossBrain.TakeDamage(amount);
        PopupEventsRenderer.OnRenderPointsAbove(new RenderEvent{
            eventType = EventType.BossHealth,
            value = -amount,
            fatherTransform = null,
            position = transform.position,
            OnFinish = null
        });
    }
}