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
                SpawnTargetedIcicle();
            }

            float waitTime = Random.Range(MinWaitTime, MaxWaitTime) / Mathf.Max(0.1f, spawnRateMultiplier);
            yield return new WaitForSeconds(waitTime);
        }
    }

    private void SpawnTargetedIcicle()
    {
        Transform target = GetClosestPlayer();
        IcicleLogic selectedIcicle = _iciclePool.Find(i => !i.gameObject.activeSelf);

        Vector3 spawnPos = GetSplinePointAtX(target.position.x);

        selectedIcicle.transform.position = spawnPos;
        selectedIcicle.transform.rotation = Quaternion.identity;

        selectedIcicle.gameObject.SetActive(true);
        selectedIcicle.ActivateFall();
        
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

    private Transform GetClosestPlayer()
    {
        if (!player1) return player2;
        if (!player2) return player1;

        float p1Dist = Mathf.Abs(player1.position.x - transform.position.x);
        float p2Dist = Mathf.Abs(player2.position.x - transform.position.x);

        return p1Dist < p2Dist ? player1 : player2;
    }
}