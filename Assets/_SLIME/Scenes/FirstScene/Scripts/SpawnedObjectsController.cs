using System;
using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Serialization;

public class SpawnedObjectsController : MonoBehaviour
{
    [Header("Spawned Objects Settings")]
    [SerializeField] private float timeBetweenSpawns = 5f;

    [Header("Camera Settings")]
    [SerializeField] private Camera mainCamera;
    [SerializeField] private float zoomOutAmount = 1.0f;
    [SerializeField] private Ease zoomEase = Ease.OutQuad;
    [SerializeField] private float zoomSpeed = 5f;

    [Header("Bloom Settings")]
    [SerializeField] private VolumeProfile bloomVolume;
    [SerializeField] private float bloomFinalIntensity = 100f;
    [SerializeField] private float bloomBaseIntensity;
    
    [Header("Player Input Settings")]
    [SerializeField] private UnityEngine.InputSystem.PlayerInput playerInput;

    [SerializeField] private GameObject transition;
    
    private Bloom _bloom;

    private void OnEnable()
    {
        
        foreach (Transform child in transform)
        {
            child.gameObject.SetActive(false);
        }
        
        if (bloomVolume.TryGet(out Bloom bloomComponent))
        {
            _bloom = bloomComponent;
            _bloom.intensity.value = bloomBaseIntensity;
        }
        
        StartCoroutine(SpawnRoutine());
    }

    // ReSharper disable Unity.PerformanceAnalysis
    private IEnumerator SpawnRoutine()
    {
        int totalChildren = transform.childCount;
        int currentChild = 0;

        foreach (Transform child in transform)
        {
            yield return new WaitForSeconds(timeBetweenSpawns);
            
            
            currentChild++;
            child.gameObject.SetActive(true);
            float targetSize = mainCamera.orthographicSize + zoomOutAmount;
            mainCamera.DOOrthoSize(targetSize, zoomSpeed).SetEase(zoomEase);
            
            float progress = (float)currentChild / totalChildren;
            float newBaseIntensity = Mathf.Lerp(bloomBaseIntensity, bloomFinalIntensity, progress);

            DOTween.To(() => _bloom.intensity.value, x =>
                _bloom.intensity.value = x, newBaseIntensity, timeBetweenSpawns).SetEase(Ease.Linear);
            
            if (currentChild != 1) continue;
            yield return new WaitForSeconds(timeBetweenSpawns);
            child.gameObject.GetComponent<UnityEngine.InputSystem.PlayerInput>().enabled = true;
            child.gameObject.GetComponent<FloatingObject>().enabled = false;
        }

        yield return new WaitForSeconds(timeBetweenSpawns);
        transition.SetActive(true);
    }

    private void OnDisable()
    {
        _bloom.intensity.value = bloomBaseIntensity;
    }
}