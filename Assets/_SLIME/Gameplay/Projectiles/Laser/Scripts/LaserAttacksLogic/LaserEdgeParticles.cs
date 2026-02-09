using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaserEdgeManager : MonoBehaviour
{
    [Header("Assets")]
    public GameObject particlePrefab;

    [Header("Laser Setup")]
    [Tooltip("Manually drag the GameObjects that have the PolygonColliders here.")]
    public List<GameObject> laserObjects = new List<GameObject>();

    [Header("Settings")]
    public float edgeBuffer = 0.01f;
    public float intersectionEpsilon = 0.001f;

    private class LaserTrack
    {
        public PolygonCollider2D poly;
        public ParticleSystem sparks;
        public Transform sparksTransform;
        public GameObject particleObject;
    }

    private List<LaserTrack> lasers = new List<LaserTrack>();
    private Camera mainCam;

    private void Awake()
    {
        mainCam = Camera.main;
    }

    private void OnEnable()
    {
        // Re-initialize every time the manager is turned on
        SetupLasers();
    }

    private void OnDisable()
    {
        // Destroy everything to ensure a clean reset
        CleanupParticles();
    }

    private void SetupLasers()
    {
        CleanupParticles(); // Safety clear

        foreach (GameObject obj in laserObjects)
        {
            if (obj == null) continue;

            PolygonCollider2D col = obj.GetComponent<PolygonCollider2D>();
            if (col == null)
            {
                Debug.LogWarning($"Object {obj.name} assigned to LaserEdgeManager has no PolygonCollider2D!");
                continue;
            }

            LaserTrack track = new LaserTrack();
            track.poly = col;

            if (particlePrefab != null)
            {
                // FIX: Instantiate as child of THIS Manager (transform), not the laser collider (col.transform)
                // This prevents the particles from inheriting the Laser's Scale or Active state.
                GameObject p = Instantiate(particlePrefab, transform); 
                
                track.particleObject = p;
                track.sparks = p.GetComponent<ParticleSystem>();
                track.sparksTransform = p.transform;

                var main = track.sparks.main;
                main.simulationSpace = ParticleSystemSimulationSpace.World;
                
                // Ensure the particle object is active so we can control the system manually
                p.SetActive(true); 
                
                track.sparks.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            }
            lasers.Add(track);
        }
    }

    private void CleanupParticles()
    {
        foreach (var laser in lasers)
        {
            if (laser != null && laser.particleObject != null)
            {
                Destroy(laser.particleObject);
            }
        }
        lasers.Clear();
    }

    private void LateUpdate()
    {
        if (mainCam == null) mainCam = Camera.main;
        if (mainCam == null || lasers.Count == 0) return;

        // Screen bounds logic...
        Vector3 bottomLeft = mainCam.ViewportToWorldPoint(new Vector3(0, 0, mainCam.nearClipPlane));
        Vector3 topRight = mainCam.ViewportToWorldPoint(new Vector3(1, 1, mainCam.nearClipPlane));

        float minX = bottomLeft.x;
        float maxX = topRight.x;
        float minY = bottomLeft.y;
        float maxY = topRight.y;

        foreach (var laser in lasers)
        {
            // --- UPDATED CHECK ---
            // If the collider is null, the game object is disabled, OR the collider component is unchecked:
            if (laser.poly == null || !laser.poly.gameObject.activeInHierarchy || !laser.poly.enabled)
            {
                // Remove '&& laser.sparks.isPlaying' here. 
                // We want to force a "Clear" even if the system thinks it's already stopped (e.g. fading out).
                if (laser.sparks != null) 
                {
                    laser.sparks.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                }
                continue;
            }

            // ... Intersection Logic ... 
            Vector2 hitPoint;
            Vector2 wallNormal;

            if (FindExactIntersection(laser.poly, minX, maxX, minY, maxY, out hitPoint, out wallNormal))
            {
                Vector2 finalPos = hitPoint + (wallNormal * edgeBuffer);
                laser.sparksTransform.position = new Vector3(finalPos.x, finalPos.y, laser.poly.transform.position.z);
                laser.sparksTransform.up = wallNormal;

                if (!laser.sparks.isPlaying) 
                    laser.sparks.Play();
            }
            else
            {
                // If aiming at nothing, we just Stop (allow fade out), we don't force Clear.
                if (laser.sparks.isPlaying) 
                    laser.sparks.Stop();
            }
        }
    }

    // --- MATH LOGIC ---

    private bool FindExactIntersection(PolygonCollider2D poly, float minX, float maxX, float minY, float maxY, out Vector2 hit, out Vector2 normal)
    {
        hit = Vector2.zero;
        normal = Vector2.up;
        bool found = false;
        float bestDist = float.MaxValue;

        Vector2 origin = poly.transform.position;
        Vector2[] points = poly.points;
        Transform t = poly.transform;

        for (int i = 0; i < points.Length; i++)
        {
            Vector2 p1 = t.TransformPoint(points[i]);
            Vector2 p2 = t.TransformPoint(points[(i + 1) % points.Length]);

            UpdateHit(p1, p2, new Vector2(minX, minY), new Vector2(minX, maxY), Vector2.right, origin, ref bestDist, ref hit, ref normal, ref found);
            UpdateHit(p1, p2, new Vector2(maxX, minY), new Vector2(maxX, maxY), Vector2.left, origin, ref bestDist, ref hit, ref normal, ref found);
            UpdateHit(p1, p2, new Vector2(minX, minY), new Vector2(maxX, minY), Vector2.up, origin, ref bestDist, ref hit, ref normal, ref found);
            UpdateHit(p1, p2, new Vector2(minX, maxY), new Vector2(maxX, maxY), Vector2.down, origin, ref bestDist, ref hit, ref normal, ref found);
        }

        return found;
    }

    private void UpdateHit(Vector2 a1, Vector2 a2, Vector2 b1, Vector2 b2, Vector2 wallNormal, Vector2 origin,
                                ref float bestDist, ref Vector2 bestHit, ref Vector2 bestNormal, ref bool found)
    {
        if (LineIntersection(a1, a2, b1, b2, out Vector2 p))
        {
            float d = Vector2.Distance(origin, p);
            if (d < bestDist)
            {
                bestDist = d;
                bestHit = p;
                bestNormal = wallNormal;
                found = true;
            }
        }
    }

    private bool LineIntersection(Vector2 a1, Vector2 a2, Vector2 b1, Vector2 b2, out Vector2 intersection)
    {
        intersection = Vector2.zero;
        float d = (a2.x - a1.x) * (b2.y - b1.y) - (a2.y - a1.y) * (b2.x - b1.x);
        if (Mathf.Abs(d) < 0.00001f) return false;

        float t = ((b1.x - a1.x) * (b2.y - b1.y) - (b1.y - a1.y) * (b2.x - b1.x)) / d;
        float u = ((b1.x - a1.x) * (a2.y - a1.y) - (b1.y - a1.y) * (a2.x - a1.x)) / d;

        if (t >= -intersectionEpsilon && t <= 1f + intersectionEpsilon && 
            u >= -intersectionEpsilon && u <= 1f + intersectionEpsilon)
        {
            intersection = a1 + Mathf.Clamp01(t) * (a2 - a1);
            return true;
        }
        return false;
    }
}