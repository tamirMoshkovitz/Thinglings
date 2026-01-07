using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class IcicleSpawner : MonoBehaviour
{
    [Header("Spawn Timing")]
    [Tooltip("Minimum time to wait before the next spawn.")]
    [SerializeField] private float minWaitTime = 1.0f;
    [Tooltip("Maximum time to wait before the next spawn.")]
    [SerializeField] private float maxWaitTime = 3.0f;

    [Header("Behavior")]
    [Tooltip("If true, it will keep spawning indefinitely.")]
    [SerializeField] private bool loopSpawning = true;

    private readonly List<GameObject> _iciclePool = new List<GameObject>();
    private Coroutine _spawnCoroutine;
    private float _timer;
    private void OnEnable()
    {
        foreach (Transform child in transform)
        {
            child.gameObject.SetActive(false);
            _iciclePool.Add(child.gameObject);
        }
        StartSpawning();
    }

    public void OnSecondStageStart()
    {
        gameObject.SetActive(false);
    }

    private void StartSpawning()
    {
        if (_spawnCoroutine != null) StopCoroutine(_spawnCoroutine);
        _spawnCoroutine = StartCoroutine(SpawnRoutine());
    }

    public void StopSpawning()
    {
        if (_spawnCoroutine != null) StopCoroutine(_spawnCoroutine);
    }

    private IEnumerator SpawnRoutine()
    {
        yield return new WaitForSeconds(Random.Range(minWaitTime, maxWaitTime));

        while (loopSpawning)
        {
            SpawnRandomIcicle();
            float waitTime = Random.Range(minWaitTime, maxWaitTime);
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
    }
}