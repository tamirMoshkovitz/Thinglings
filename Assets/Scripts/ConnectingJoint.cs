using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class ConnectingJoint : MonoBehaviour
{
    #region Joint Args
    [SerializeField] private int maxConnections = 2;
    [SerializeField] private float detectionRadius = 1f;
    [SerializeField] private string connectableTag = "Connectable";
    [SerializeField] private bool syncColliderRadius;
    [SerializeField] private float checkInterval = 0.2f;
    [SerializeField] private float maxBreakForce = 80f;
    [SerializeField] private float frequency = 2f;
    [SerializeField] private float dampingRatio = 0.2f;
    #endregion
    

    private float _checkConnectionTimer;
    private Rigidbody2D _rb;
    private CircleCollider2D _circleCollider;
    private List<SpringJoint2D> _joints;
    private List<GameObject> _connectedObjects;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        if (_rb != null)
            _rb.interpolation = RigidbodyInterpolation2D.Interpolate;

        _circleCollider = GetComponent<CircleCollider2D>();
        if (syncColliderRadius && _circleCollider != null)
            _circleCollider.radius = detectionRadius;

        _joints = new List<SpringJoint2D>(maxConnections);
        _connectedObjects = new List<GameObject>(maxConnections);
    }

    private void Update()
    {
        _checkConnectionTimer += Time.deltaTime;
        if (!(_checkConnectionTimer >= checkInterval)) return;
        _checkConnectionTimer = 0f;
        UpdateConnections();
    }

    // ReSharper disable Unity.PerformanceAnalysis
    private void UpdateConnections()
    {
        CleanupBrokenJoints();
        ConnectClosestUpToMax();
    }

    private void CleanupBrokenJoints()
    {
        if (_connectedObjects.Count == 0) return;

        for (var i = _connectedObjects.Count - 1; i >= 0; i--)
        {
            if (!_joints[i])
            {
                RemoveConnectionAt(i);
                continue;
            }

            if (_joints[i].connectedBody) continue;
            RemoveConnectionAt(i, false);
        }
    }
    

    private bool TryAddingConnection(GameObject target)
    {
        if (target == null) return false;
        if (_connectedObjects.Count >= maxConnections) return false;
        if (_connectedObjects.Contains(target)) return false;

        Rigidbody2D targetRb = target.GetComponent<Rigidbody2D>();
        if (targetRb == null) return false;

        var joint = gameObject.AddComponent<SpringJoint2D>();
        joint.connectedBody = targetRb;
        joint.autoConfigureDistance = true;
        joint.dampingRatio = dampingRatio;
        joint.frequency = frequency;
        joint.breakForce = maxBreakForce;

        _joints.Add(joint);
        _connectedObjects.Add(target);
        return true;
    }
    
    private void ConnectClosestUpToMax()
    {
        int totalConnectedObjects = maxConnections - _connectedObjects.Count;
        if (totalConnectedObjects <= 0) return;

        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, detectionRadius);
        var optionalConnections = new List<(GameObject gameObject, float distance)>();

        foreach (var hit in hits)
        {
            if (hit == null) continue;
            if (!hit.CompareTag(connectableTag)) continue;
            var collidedGameObject = hit.gameObject;
            if (collidedGameObject == gameObject) continue;
            if (_connectedObjects.Contains(collidedGameObject)) continue;
            var rb = collidedGameObject.GetComponent<Rigidbody2D>();
            if (rb == null) continue;
            if (transform.parent != null && transform.parent == collidedGameObject.transform.parent) continue;

            var dist = (collidedGameObject.transform.position - transform.position).sqrMagnitude;
            optionalConnections.Add((collidedGameObject, dist));
        }

        if (optionalConnections.Count == 0) return;
        optionalConnections.Sort((a, b) => a.distance.CompareTo(b.distance));

        var connected = 0;
        for (var i = 0; i < optionalConnections.Count && connected < totalConnectedObjects; i++)
        {
            if (TryAddingConnection(optionalConnections[i].gameObject))
                connected++;
        }
    }

    private void RemoveConnectionAt(int idx, bool destroyJoint = true)
    {
        if (idx < 0 || idx >= _joints.Count) return;

        var joint = _joints[idx];
        _joints.RemoveAt(idx);
        _connectedObjects.RemoveAt(idx);

        if (destroyJoint && joint != null)
        {
            Destroy(joint);
        }
        
        // TODO: Elad or tamir add an event here to notify connection removed - sound and vibration
    }

    private void ClearConnections()
    {
        foreach (var j in _joints)
            if (j != null) Destroy(j);
        _joints.Clear();
        _connectedObjects.Clear();
    }

    private void OnDestroy()
    {
        ClearConnections();
    }
}