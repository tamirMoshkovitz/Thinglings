using System;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class FitCameraToFullHD : MonoBehaviour
{
    private const float TargetAspect = 1920f / 1080f;  // Full HD (16:9)
    private const float RatioChangeThreshold = 0.01f;

    [SerializeField] private Camera cam;

    [Header("World units that span the width at 1080p height")]
    [SerializeField] private float worldWidth = 40f;

    private float _lastScreenRatio;

    private void Awake()
    {
        if (cam == null)
            cam = GetComponent<Camera>();

        if (!cam.orthographic)
            Debug.LogWarning("Camera is not orthographic. This script expects an orthographic camera.");
    }

    private void Start()
    {
        ApplyAspectCorrection();
        FitCameraSize();
    }

    private void Update()
    {
        float currentRatio = (float)Screen.width / Screen.height;

        if (Math.Abs(currentRatio - _lastScreenRatio) > RatioChangeThreshold)
        {
            ApplyAspectCorrection();
            FitCameraSize();
        }
    }

    private void ApplyAspectCorrection()
    {
        float windowAspect = (float)Screen.width / Screen.height;
        float scaleHeight = windowAspect / TargetAspect;

        if (scaleHeight < 1f)
        {
            // Letterbox (black bars top/bottom)
            Rect rect = cam.rect;
            rect.width = 1f;
            rect.height = scaleHeight;
            rect.x = 0;
            rect.y = (1f - scaleHeight) * 0.5f;
            cam.rect = rect;
        }
        else
        {
            // Pillarbox (black bars left/right)
            float scaleWidth = 1f / scaleHeight;
            Rect rect = cam.rect;
            rect.width = scaleWidth;
            rect.height = 1f;
            rect.x = (1f - scaleWidth) * 0.5f;
            rect.y = 0;
            cam.rect = rect;
        }

        _lastScreenRatio = windowAspect;
    }

    private void FitCameraSize()
    {
        float targetWidth = worldWidth;
        float orthographicSize = targetWidth / (2f * TargetAspect);
        cam.orthographicSize = orthographicSize;
    }
}