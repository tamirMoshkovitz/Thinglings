using System.Collections.Generic;
using _SLIME.Laser;
using UnityEngine;

public class LaserEdgeParticles : MonoBehaviour
{
    [Header("Assets")]
    public GameObject particlePrefab;

    [Header("Settings")]
    public float edgeBuffer = 0.5f;
    public bool showDualSparks = true;

    private class LaserTracker
    {
        public Collider2D Collider;
        public Transform Transform;
        public ParticleSystem SparksForward;
        public ParticleSystem SparksBackward; 
        public bool IsVerticalArt;
    }

    private List<LaserTracker> _lasers = new List<LaserTracker>();
    private Camera _mainCam;
    private LaserAttackLogic _logicScript;
    private float _minX, _maxX, _minY, _maxY;

    private void Start()
    {
        _mainCam = Camera.main;
        _logicScript = GetComponent<LaserAttackLogic>();

        Collider2D[] allColliders = GetComponentsInChildren<Collider2D>(true);
        GameObject container = new GameObject("LaserSparks_Container");

        foreach (var col in allColliders)
        {
            if (col.gameObject == gameObject || !col.isTrigger) continue;

            LaserTracker tracker = new LaserTracker();
            tracker.Collider = col;
            tracker.Transform = col.transform;

            if (col is BoxCollider2D box)
                tracker.IsVerticalArt = box.size.y > box.size.x;
            else if (col is CapsuleCollider2D cap)
                tracker.IsVerticalArt = cap.direction == CapsuleDirection2D.Vertical;

            tracker.SparksForward = CreateSpark(container, col.name + "_Sparks_F", col.transform.position.z);
            tracker.SparksBackward = CreateSpark(container, col.name + "_Sparks_B", col.transform.position.z);

            _lasers.Add(tracker);
        }
    }

    private ParticleSystem CreateSpark(GameObject container, string name, float z)
    {
        if (!particlePrefab) return null;
        var obj = Instantiate(particlePrefab, container.transform);
        obj.name = name;
        obj.transform.position = new Vector3(0, 0, z);
        var ps = obj.GetComponent<ParticleSystem>();
        ps.Stop();
        return ps;
    }

    private void LateUpdate()
    {
        if (!_mainCam) return;
        if (_logicScript && !_logicScript.enabled) { StopAll(); return; }

        UpdateBounds();

        foreach (var laser in _lasers)
        {
            // Individual Toggle Check: If collider is off, sparks are off
            if (!laser.Collider || !laser.Collider.enabled || !laser.Collider.gameObject.activeInHierarchy)
            {
                if (laser.SparksForward.isPlaying) laser.SparksForward.Stop();
                if (laser.SparksBackward.isPlaying) laser.SparksBackward.Stop();
                continue;
            }

            Vector2 forwardDir = laser.IsVerticalArt ? laser.Transform.up : laser.Transform.right;

            UpdateSystem(laser.SparksForward, laser.Transform.position, forwardDir);

            if (showDualSparks)
            {
                UpdateSystem(laser.SparksBackward, laser.Transform.position, -forwardDir);
            }
            else if (laser.SparksBackward.isPlaying)
            {
                laser.SparksBackward.Stop();
            }
        }
    }

    void UpdateSystem(ParticleSystem ps, Vector3 origin, Vector2 dir)
    {
        if (!ps) return;

        Vector2 hit, normal;
        if (CalculateIntersection(origin, dir, out hit, out normal))
        {
            ps.transform.position = new Vector3(hit.x, hit.y, ps.transform.position.z);
            if (normal != Vector2.zero) {
                ps.transform.rotation = Quaternion.identity;
                ps.transform.up = normal;
            }

            if (!ps.isPlaying) ps.Play();
        }
        else
        {
            if (ps.isPlaying) ps.Stop();
        }
    }

    void StopAll()
    {
        foreach (var l in _lasers)
        {
            if (l.SparksForward.isPlaying) l.SparksForward.Stop();
            if (l.SparksBackward.isPlaying) l.SparksBackward.Stop();
        }
    }

    void UpdateBounds()
    {
        float v = _mainCam.orthographicSize;
        float h = v * _mainCam.aspect;
        Vector2 c = _mainCam.transform.position;
        _minX = c.x - h; _maxX = c.x + h; _minY = c.y - v; _maxY = c.y + v;
    }

    bool CalculateIntersection(Vector2 origin, Vector2 dir, out Vector2 hit, out Vector2 norm)
    {
        float dist = float.MaxValue;
        hit = origin;
        norm = Vector2.zero;
        bool found = CheckWall(_minX, true, Vector2.right, origin, dir, ref dist, ref hit, ref norm);
        if (CheckWall(_maxX, true, Vector2.left, origin, dir, ref dist, ref hit, ref norm)) found = true;
        if (CheckWall(_minY, false, Vector2.up, origin, dir, ref dist, ref hit, ref norm)) found = true;
        if (CheckWall(_maxY, false, Vector2.down, origin, dir, ref dist, ref hit, ref norm)) found = true;

        return found;
    }

    bool CheckWall(float wall, bool isVert, Vector2 wallNorm, Vector2 origin, Vector2 dir, ref float bestDist, ref Vector2 bestHit, ref Vector2 bestNorm)
    {
        float t = -1f;
        if (isVert && Mathf.Abs(dir.x) > 0.001f) t = (wall - origin.x) / dir.x;
        else if (!isVert && Mathf.Abs(dir.y) > 0.001f) t = (wall - origin.y) / dir.y;

        if (t > 0 && t < bestDist)
        {
            Vector2 p = origin + (dir * t);
            if (p.x >= _minX - 0.1f && p.x <= _maxX + 0.1f && p.y >= _minY - 0.1f && p.y <= _maxY + 0.1f)
            {
                bestDist = t;
                bestHit = p - (dir * edgeBuffer);
                bestNorm = wallNorm;
                return true;
            }
        }
        return false;
    }
}