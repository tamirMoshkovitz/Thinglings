using UnityEngine;
using UnityEngine.VFX;
using System.Collections;
using System.Collections.Generic;
using _SLIME.Gameplay.Slime.Scripts.new_scripts;

public class LaserAttackLogic : MonoBehaviour
{
    [System.Serializable]
    public class AnimationProfile
    {
        public float duration = 1.0f; // Set this higher (e.g. 3.0) to grow slower!
        public AnimationCurve curve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    }

    [Header("VFX Settings")]
    [Tooltip("The name of the Float property in VFX Graph (e.g. 'Length')")]
    [SerializeField] private string _vfxPropertyName = "Length";
    
    [Tooltip("If TRUE: Sends the actual distance (0 to 50) to the graph.\nIf FALSE: Sends 0 to 1 (good for Alpha/Opacity).")]
    [SerializeField] private bool _useDistanceForVisuals = true; 

    [Header("Raycast Settings")]
    [Tooltip("Adjust to match laser direction. (0,1,0) = Up, (0,0,1) = Forward")]
    [SerializeField] private Vector3 _localLaserDirection = Vector3.up; 
    [SerializeField] private float _maxDistance = 50f;
    [SerializeField] private LayerMask _collisionLayer;
    [SerializeField] private bool _showDebugLines = true;

    private VisualEffect[] _lasers;
    private bool _isSpinning;
    private float _currentRotationSpeed;

    private HashSet<VisualEffect> _activeLasers = new HashSet<VisualEffect>();
    private Dictionary<VisualEffect, float> _rayLengths = new Dictionary<VisualEffect, float>();

    private void Awake()
    {
        _lasers = GetComponentsInChildren<VisualEffect>();
        foreach (var laser in _lasers) _rayLengths[laser] = 0f;
        gameObject.SetActive(false); 
    }

    private void Update()
    {
        if (_isSpinning)
            transform.Rotate(Vector3.forward * (_currentRotationSpeed * Time.deltaTime));

        if (_activeLasers.Count > 0)
            PerformDynamicLaserRaycasts();
    }

    public IEnumerator PlayGrowSequence(AnimationProfile profile, float staggerDelay)
    {
        gameObject.SetActive(true); 
        foreach (var laser in _lasers)
        {
            StartCoroutine(AnimateLaserExplicit(laser, profile, true));
            yield return new WaitForSeconds(staggerDelay);
        }
    }

    public IEnumerator PlayDissolveSequence(AnimationProfile profile, float staggerDelay)
    {
        foreach (var laser in _lasers)
        {
            StartCoroutine(AnimateLaserExplicit(laser, profile, false));
            yield return new WaitForSeconds(staggerDelay);
        }
    }

    private IEnumerator AnimateLaserExplicit(VisualEffect laser, AnimationProfile profile, bool isGrowing)
    {
        float startRay = isGrowing ? 0f : _maxDistance;
        float endRay   = isGrowing ? _maxDistance : 0f;

        if (isGrowing)
        {
            if (!_activeLasers.Contains(laser)) _activeLasers.Add(laser);
        }

        float timer = 0f;
        float safeDuration = Mathf.Max(profile.duration, 0.01f);

        while (timer < 1f)
        {
            timer += Time.deltaTime / safeDuration;
            
            float curveVal = profile.curve.Evaluate(timer);

            float currentLength = Mathf.Lerp(startRay, endRay, curveVal);

            _rayLengths[laser] = currentLength;

            if (_useDistanceForVisuals)
            {
                laser.SetFloat(_vfxPropertyName, currentLength);
            }
            else
            {
                float normalizedVal = Mathf.Lerp(isGrowing ? 0f : 1f, isGrowing ? 1f : 0f, curveVal);
                laser.SetFloat(_vfxPropertyName, normalizedVal);
            }

            yield return null;
        }

        _rayLengths[laser] = endRay;
        laser.SetFloat(_vfxPropertyName, _useDistanceForVisuals ? endRay : (isGrowing ? 1f : 0f));

        if (!isGrowing)
        {
            if (_activeLasers.Contains(laser)) _activeLasers.Remove(laser);
        }
    }

    public void ResetVisuals()
    {
        _activeLasers.Clear(); 
        foreach (var laser in _lasers)
        {
            laser.SetFloat(_vfxPropertyName, 0f);
            _rayLengths[laser] = 0f;
        }
    }

    public void SetSpinning(bool active, float speed)
    {
        _currentRotationSpeed = speed;
        _isSpinning = active;
        if(active) gameObject.SetActive(true);
    }

    private void PerformDynamicLaserRaycasts()
    {
        foreach (var laser in _activeLasers)
        {
            if (!laser) continue;
            if (!_rayLengths.TryGetValue(laser, out float currentLen) || currentLen < 0.1f) continue;

            Vector3 origin = laser.transform.position;
            Vector3 direction = laser.transform.TransformDirection(_localLaserDirection);
            RaycastHit hit;

            bool didHit = Physics.Raycast(origin, direction, out hit, currentLen, _collisionLayer);

            if (didHit)
            {
                if(hit.transform == transform || hit.transform.IsChildOf(transform)) continue;
            }
        }
    }

    private void OnDrawGizmos()
    {
        if (!_showDebugLines) return;
        var lasersToShow = Application.isPlaying ? _activeLasers : new HashSet<VisualEffect>(GetComponentsInChildren<VisualEffect>());
        if (lasersToShow == null) return;

        foreach (var laser in lasersToShow)
        {
            if (laser == null) continue;
            Gizmos.color = Color.red;
            Vector3 origin = laser.transform.position;
            Vector3 direction = laser.transform.TransformDirection(_localLaserDirection);
            
            float len = (Application.isPlaying && _rayLengths.ContainsKey(laser)) ? _rayLengths[laser] : _maxDistance;

            if (len > 0.1f)
            {
                Gizmos.DrawRay(origin, direction * len);
            }
        }
    }
}