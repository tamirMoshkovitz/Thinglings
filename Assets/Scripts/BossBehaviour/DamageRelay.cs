using System;
using UI;
using UnityEngine;
using EventType = UI.EventType;

public class DamageRelay : MonoBehaviour
{
    public BossController bossBrain; 

    // Note: We don't need "isLeft" anymore because the health is shared!
    
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

    public void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log("took damage from " + other.name + " from Layer " + LayerMask.LayerToName(other.gameObject.layer));
        TakeDamage(1f);
    }

    // Test Clicker
    private void OnMouseDown()
    {
        TakeDamage(10);
    }
}