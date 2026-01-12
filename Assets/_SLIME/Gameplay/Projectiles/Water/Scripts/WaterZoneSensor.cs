using UnityEngine;

public class WaterZoneSensor : MonoBehaviour
{
    [SerializeField] private WaterAttackManager manager;
    
    // We assign an ID (0 or 1) in the inspector so the manager knows which one this is
    [SerializeField] private int sensorId; 

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            manager.SetZoneState(sensorId, true);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            manager.SetZoneState(sensorId, false);
        }
    }
}