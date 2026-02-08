using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Splines;
using Unity.Mathematics;
using _SLIME.Boss;
using _SLIME.Core.Audio.FMOD.Scripts;
using FMODUnity;
using Random = UnityEngine.Random;

public class IcicleSpawner : MonoBehaviour
{
    [Header("Pool Settings")]
    [SerializeField] private GameObject iciclePrefab;
    [SerializeField] private int poolSize = 10;

    [Header("Targeting")]
    [SerializeField] private Transform player1;
    [SerializeField] private Transform player2;

    [Header("Phase Filtering")]
    [Tooltip("Add the states here where icicles ARE allowed to spawn.")]
    [SerializeField] private List<BossStates> spawnableStates = new List<BossStates> { BossStates.FarState };

    [Header("Path Settings")]
    [SerializeField] private SplineContainer splineContainer;
    [SerializeField] private float searchResolution = 20f; 

    [Header("Settings")]
    public float spawnRateMultiplier = 1f;
    [SerializeField] private BossBrain bossBrain;
    [SerializeField] private EventReference icicleSpawnSfx; 

    private readonly List<IcicleLogic> _iciclePool = new List<IcicleLogic>();
    private Coroutine _spawnCoroutine;

    private float MinWaitTime => BossBrain.bossConfigurations.IcicleSpawn.minWaitTime;
    private float MaxWaitTime => BossBrain.bossConfigurations.IcicleSpawn.maxWaitTime;
    private bool LoopSpawning => BossBrain.bossConfigurations.IcicleSpawn.loopSpawning;

    private void Awake()
    {
        for (int i = 0; i < poolSize; i++)
        {
            GameObject obj = Instantiate(iciclePrefab, transform);
            _iciclePool.Add(obj.GetComponent<IcicleLogic>());
            obj.SetActive(false);
        }
    }

    private void OnEnable()
    {
        TunnelPhaseState.TunnelPhaseStarted += OnTunnelPhaseStarted;
        StartSpawning();
    }

    private void OnDisable() => TunnelPhaseState.TunnelPhaseStarted -= OnTunnelPhaseStarted;
    private void OnTunnelPhaseStarted() => gameObject.SetActive(false);

    private void StartSpawning()
    {
        if (_spawnCoroutine != null) StopCoroutine(_spawnCoroutine);
        _spawnCoroutine = StartCoroutine(SpawnRoutine());
    }

    // ReSharper disable Unity.PerformanceAnalysis
    private IEnumerator SpawnRoutine()
    {
        while (!BossBrain.bossConfigurations) yield return null;
        yield return new WaitForSeconds(Random.Range(MinWaitTime, MaxWaitTime) / spawnRateMultiplier);

        while (LoopSpawning)
        {
            if (spawnableStates.Contains(BossBrain.BossState))
            {
                yield return StartCoroutine(SpawnIcicleWave());
            }

            float waitTime = Random.Range(MinWaitTime, MaxWaitTime) / Mathf.Max(0.1f, spawnRateMultiplier);
            yield return new WaitForSeconds(waitTime);
        }
    }

    private IEnumerator SpawnIcicleWave()
    {
        var settings = BossBrain.bossConfigurations.IcicleSpawn;
        int count = Mathf.Clamp(Random.Range(settings.minIcicleCount, settings.maxIcicleCount + 1), 1, _iciclePool.Count);
        if (count <= 0) yield break;

        List<IcicleLogic> available = new List<IcicleLogic>();
        foreach (var i in _iciclePool)
        {
            if (!i.gameObject.activeSelf) available.Add(i);
            if (available.Count >= count) break;
        }
        if (available.Count == 0) yield break;
        count = Mathf.Min(count, available.Count);

        float playerTargetX = GetClosestPlayer().position.x;
        (float minX, float maxX) = GetSplineWorldXRange();
        float minSpacing = Mathf.Max(0f, settings.minSpacingBetweenIcicles);
        float minDelay = Mathf.Max(0f, settings.minDelayBetweenIcicles);
        float maxDelay = Mathf.Max(minDelay, settings.maxDelayBetweenIcicles);

        List<float> xPositions = new List<float>(count);
        const int maxAttemptsPerIcicle = 50;
        for (int i = 0; i < count; i++)
        {
            float x = 0f;
            bool valid = false;
            for (int attempt = 0; attempt < maxAttemptsPerIcicle; attempt++)
            {
                if (Random.value < settings.playerTargetChance)
                {
                    float deviation = Random.Range(-settings.accuracyOffset, settings.accuracyOffset);
                    x = Mathf.Clamp(playerTargetX + deviation, minX, maxX);
                }
                else
                {
                    x = Random.Range(minX, maxX);
                }
                valid = true;
                foreach (float existing in xPositions)
                {
                    if (Mathf.Abs(x - existing) < minSpacing)
                    {
                        valid = false;
                        break;
                    }
                }
                if (valid) break;
            }
            if (!valid) continue;
            xPositions.Add(x);
        }

        for (int i = 0; i < xPositions.Count; i++)
        {
            if (i > 0)
                yield return new WaitForSeconds(Random.Range(minDelay, maxDelay));

            Vector3 spawnPos = GetSplinePointAtX(xPositions[i]);
            IcicleLogic icicle = available[i];
            icicle.transform.position = spawnPos;
            icicle.transform.rotation = Quaternion.identity;
            icicle.gameObject.SetActive(true);
            icicle.ActivateFall();
            
        }
        SFXPlayer.Play(icicleSpawnSfx);
    }

    private Vector3 GetSplinePointAtX(float targetWorldX)
    {
        var spline = splineContainer.Spline;
        float bestT = 0;
        float closestXDist = float.MaxValue;
        float length = spline.GetLength();
        int samples = Mathf.Max(30, (int)(length * searchResolution));

        for (int i = 0; i <= samples; i++)
        {
            float t = i / (float)samples;
            float3 localPos = spline.EvaluatePosition(t);
            Vector3 worldPos = splineContainer.transform.TransformPoint(localPos);

            float xDist = Mathf.Abs(worldPos.x - targetWorldX);
            if (xDist < closestXDist)
            {
                closestXDist = xDist;
                bestT = t;
            }
        }

        Vector3 finalPos = splineContainer.transform.TransformPoint(spline.EvaluatePosition(bestT));
        
        finalPos.z = 0;
        return finalPos;
    }

    private (float minX, float maxX) GetSplineWorldXRange()
    {
        var spline = splineContainer.Spline;
        float length = spline.GetLength();
        int samples = Mathf.Max(20, (int)(length * searchResolution));
        float minX = float.MaxValue;
        float maxX = float.MinValue;
        for (int i = 0; i <= samples; i++)
        {
            float t = i / (float)samples;
            Vector3 worldPos = splineContainer.transform.TransformPoint(spline.EvaluatePosition(t));
            if (worldPos.x < minX) minX = worldPos.x;
            if (worldPos.x > maxX) maxX = worldPos.x;
        }
        return (minX, maxX);
    }

    private Transform GetClosestPlayer()
    {
        float p1Dist = Mathf.Abs(player1.position.x - transform.position.x);
        float p2Dist = Mathf.Abs(player2.position.x - transform.position.x);

        return p1Dist < p2Dist ? player1 : player2;
    }
}