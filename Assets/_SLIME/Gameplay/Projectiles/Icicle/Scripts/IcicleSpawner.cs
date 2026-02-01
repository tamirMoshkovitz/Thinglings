using System;
using System.Collections;
using System.Collections.Generic;
using _SLIME.Boss;
using _SLIME.Core.Audio.FMOD.Scripts;
using FMODUnity;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class IcicleSpawner : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private BossBrain bossBrain;
    [SerializeField] private EventReference icicleSpawnSfx; 

    private readonly List<GameObject> _iciclePool = new List<GameObject>();
    private Coroutine _spawnCoroutine;
    private float _timer;
    
    private float MinWaitTime => BossBrain.bossConfigurations.IcicleSpawn.minWaitTime;
    private float MaxWaitTime => BossBrain.bossConfigurations.IcicleSpawn.maxWaitTime;
    private bool LoopSpawning => BossBrain.bossConfigurations.IcicleSpawn.loopSpawning;
    private void OnEnable()
    {
        TunnelPhaseState.TunnelPhaseStarted += OnTunnelPhaseStarted;
        foreach (Transform child in transform)
        {
            child.gameObject.SetActive(false);
            _iciclePool.Add(child.gameObject);
        }
    }

    private void OnDisable()
    {
        TunnelPhaseState.TunnelPhaseStarted -= OnTunnelPhaseStarted;
    }

    private void Start()
    {
        StartSpawning();
    }

    private void OnTunnelPhaseStarted()
    {
        gameObject.SetActive(false);
    }

    private void StartSpawning()
    {
        if (_spawnCoroutine != null) StopCoroutine(_spawnCoroutine);
        _spawnCoroutine = StartCoroutine(SpawnRoutine());
    }

    private IEnumerator SpawnRoutine()
    {
        yield return new WaitForSeconds(Random.Range(MinWaitTime, MaxWaitTime));

        while (LoopSpawning)
        {
            SpawnRandomIcicle();
            float waitTime = Random.Range(MinWaitTime, MaxWaitTime);
            yield return new WaitForSeconds(waitTime);
        }
    }

    private void SpawnRandomIcicle()
    {
        List<GameObject> availableIcicles = new List<GameObject>();
        foreach (var icicle in _iciclePool)
        {
            if (!icicle.activeSelf)
            {
                availableIcicles.Add(icicle);
            }
        }
        if (availableIcicles.Count == 0) return;
        int randomIndex = Random.Range(0, availableIcicles.Count);
        GameObject selectedIcicle = availableIcicles[randomIndex];
        selectedIcicle.SetActive(true);
        
        SFXPlayer.Play(icicleSpawnSfx);
    }
}