using System.Collections.Generic;
using UnityEngine;

public class CreatureManager : MonoBehaviour
{
    [Header("Pool of Creatures")]
    public List<PeekingCreature> allCreatures = new List<PeekingCreature>();

    [Header("Timing Ranges")]
    public Vector2 spawnDelayRange = new Vector2(1f, 4f); 
    
    [Header("Settings Override")]
    public bool useGlobalHoldTime = false;
    public float globalHoldTime = 2.0f;

    private float _nextSpawnTime;

    void Update()
    {
        if (Time.time >= _nextSpawnTime)
        {
            TryTriggerCreature();
            _nextSpawnTime = Time.time + Random.Range(spawnDelayRange.x, spawnDelayRange.y);
        }
    }

    void TryTriggerCreature()
    {
        if (allCreatures.Count == 0) return;

        // ONLY look for creatures that are active in the hierarchy
        // This allows you to turn off creatures in other rooms/areas
        List<PeekingCreature> available = allCreatures.FindAll(c => c.gameObject.activeInHierarchy && !c.IsBusy);

        if (available.Count > 0)
        {
            PeekingCreature selected = available[Random.Range(0, available.Count)];
            
            if (useGlobalHoldTime) 
                selected.peekStayTime = globalHoldTime;

            selected.Peek();
        }
    }
    
    [ContextMenu("Auto-Fill Creatures")]
    void AutoFill()
    {
        allCreatures.Clear();
        // The (true) parameter finds them even if they are currently SetActive(false)
        allCreatures.AddRange(FindObjectsOfType<PeekingCreature>(true));
    }
}