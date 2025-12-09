using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Player
{
    public class Segment : MonoBehaviour
    {
        [Header("Goo / Cheese Settings")]
        public float maxThickness = 0.5f;
        public float minThickness = 0.1f;
        public AnimationCurve widthProfile;
        
        [Header("Slingshot Physics")]
        public float stiffness = 50f;
        public float damping = 3f;
        public float connectionRange = 0.5f;
        
        [Header("Performance")]
        [Tooltip("Higher = Smoother visuals, but more CPU.")]
        public int visualResolution = 40; 
        [Tooltip("Lower = Much faster physics. Keep this low (5-10).")]
        public int physicsResolution = 8; 

        [Header("General Settings")]
        public LayerMask catchableLayer;
        public float gravitySag = 1.0f;
        public float closeDistance = 1.0f;
        public float farDistance = 10.0f;
        public float tearDuration = 0.4f;

        // --- Internal Data ---
        public class TrailSegment
        {
            public LineRenderer Lr;
            public EdgeCollider2D EdgeCol;
            public Transform FromT, ToT;
            public Vector3 FromLocalPos, ToLocalPos;
            public Segment Seg;
        }

        private class CaughtObject
        {
            public Rigidbody2D Rb;
            public Transform Trans; // Cache transform access
            public float CaptureT; 
        }

        private TrailSegment trailData;
        private bool isBreaking = false;
        private List<CaughtObject> caughtObjects = new List<CaughtObject>();
        
        // --- OPTIMIZATION: Pre-allocated memory ---
        private Vector3[] m_LinePositions;     // For LineRenderer
        private List<Vector2> m_ColPositions;  // For EdgeCollider
        
        private void Start()
        {
            if (widthProfile == null || widthProfile.length == 0)
                widthProfile = new AnimationCurve(new Keyframe(0, 1), new Keyframe(0.5f, 0.3f), new Keyframe(1, 1));

            // Pre-allocate arrays to stop Garbage Collection lag
            m_LinePositions = new Vector3[visualResolution];
            m_ColPositions = new List<Vector2>(physicsResolution);
        }

        private void OnEnable()
        {
            GameEvents.BrickShot += OnBrickShot;
            GameEvents.ResetButtonPressed += OnResetButtonPressed;
            GameEvents.SlimeTears += BreakConnection;
        }
        
        private void OnDisable()
        {
            GameEvents.BrickShot -= OnBrickShot;
            GameEvents.ResetButtonPressed -= OnResetButtonPressed;
            GameEvents.SlimeTears -= BreakConnection;
        }

        private void FixedUpdate()
        {
            if (isBreaking || trailData == null) return;
            ApplySpringForces();
        }

        public void LateUpdate()
        {
            if (isBreaking || trailData == null) return;
            UpdateSegmentVisualsAndPhysics();
        }

        // ----------------------------------------------------------------
        // 1. PHYSICS FORCES
        // ----------------------------------------------------------------
        private void ApplySpringForces()
        {
            // Cache positions once per frame
            Vector3 start = trailData.FromT.TransformPoint(trailData.FromLocalPos);
            Vector3 end = trailData.ToT.TransformPoint(trailData.ToLocalPos);
            
            float dist = Vector3.Distance(start, end);
            float tension = Mathf.InverseLerp(closeDistance, farDistance, dist);
            // Pre-calculate sag scalar
            float tensionInv = Mathf.Lerp(1f, 0f, tension);

            for (int i = caughtObjects.Count - 1; i >= 0; i--)
            {
                var obj = caughtObjects[i];
                if (!obj.Rb)
                {
                    caughtObjects.RemoveAt(i);
                    continue;
                }

                // Optimization: Inline calculations
                Vector3 anchorPos = Vector3.Lerp(start, end, obj.CaptureT);
                float sagOffset = 4 * obj.CaptureT * (1 - obj.CaptureT) * gravitySag; 
                anchorPos.y -= sagOffset * tensionInv;

                Vector2 currentPos = obj.Rb.position;
                Vector2 displacement = currentPos - (Vector2)anchorPos;
                
                // Forces
                Vector2 force = -displacement * stiffness;
                Vector2 damper = -obj.Rb.linearVelocity * damping;

                obj.Rb.AddForce(force + damper);
            }
        }

        // ----------------------------------------------------------------
        // 2. VISUALS & COLLIDER (Merged for loop efficiency)
        // ----------------------------------------------------------------
        private void UpdateSegmentVisualsAndPhysics()
        {
            Vector3 startPos = trailData.FromT.TransformPoint(trailData.FromLocalPos);
            Vector3 endPos = trailData.ToT.TransformPoint(trailData.ToLocalPos);
            
            float distance = Vector3.Distance(startPos, endPos);
            float tension = Mathf.InverseLerp(closeDistance, farDistance, distance);
            float currentSag = Mathf.Lerp(gravitySag, 0f, tension);

            // Update Line Properties
            trailData.Lr.widthMultiplier = Mathf.Lerp(maxThickness, minThickness, tension);
            trailData.Lr.widthCurve = widthProfile;
            trailData.Lr.positionCount = visualResolution;

            // --- A. CALCULATE VISUALS (High Resolution) ---
            for (int i = 0; i < visualResolution; i++)
            {
                float t = (float)i / (visualResolution - 1);
                m_LinePositions[i] = CalculatePoint(t, startPos, endPos, currentSag);
            }
            // Zero-Alloc array pass
            trailData.Lr.SetPositions(m_LinePositions);


            // --- B. CALCULATE COLLIDER (Low Resolution) ---
            // Only recalculate collider if we have to. Doing this at lower res saves massive CPU.
            m_ColPositions.Clear();
            for (int i = 0; i < physicsResolution; i++)
            {
                float t = (float)i / (physicsResolution - 1);
                Vector3 worldPt = CalculatePoint(t, startPos, endPos, currentSag);
                
                // Since Segment is at 0,0,0, World Point == Local Point.
                // We skipped Transform.InverseTransformPoint completely.
                m_ColPositions.Add(worldPt); 
            }
            trailData.EdgeCol.SetPoints(m_ColPositions);
        }

        // Shared Math Logic extracted for optimization
        private Vector3 CalculatePoint(float t, Vector3 start, Vector3 end, float sag)
        {
            Vector3 pos = Vector3.Lerp(start, end, t);
            pos.y -= 4 * t * (1 - t) * sag;

            // Object Distortion Logic
            int count = caughtObjects.Count;
            if (count > 0)
            {
                for (int j = 0; j < count; j++)
                {
                    var obj = caughtObjects[j];
                    if (!obj.Trans) continue;

                    float distFromCapture = t > obj.CaptureT ? t - obj.CaptureT : obj.CaptureT - t; // Faster Abs
                    
                    if (distFromCapture < connectionRange)
                    {
                        float influence = 1f - (distFromCapture / connectionRange);
                        influence = influence * influence * (3f - 2f * influence); // SmoothStep manually (faster)

                        Vector3 neutralPos = Vector3.Lerp(start, end, t);
                        neutralPos.y -= 4 * t * (1 - t) * sag;

                        Vector3 offset = obj.Trans.position - neutralPos;
                        pos += offset * influence;
                    }
                }
            }
            return pos;
        }

        // ----------------------------------------------------------------
        // 3. CATCHING LOGIC
        // ----------------------------------------------------------------
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.gameObject.layer == LayerMask.NameToLayer("Boss"))
            {
                GameEvents.SlimeTears?.Invoke();
                BreakConnection();
            }            
            if (isBreaking) return;
            if (((1 << other.gameObject.layer) & catchableLayer) == 0) return;
            if (caughtObjects.Exists(x => x.Rb == other.attachedRigidbody)) return;

            Rigidbody2D rb = other.attachedRigidbody;
            if (rb != null)
            {
                CatchObject(rb);
            }
        }

        private void CatchObject(Rigidbody2D rb)
        {
            Vector3 start = trailData.FromT.TransformPoint(trailData.FromLocalPos);
            Vector3 end = trailData.ToT.TransformPoint(trailData.ToLocalPos);
            
            Vector3 lineDir = end - start;
            Vector3 hitVec = rb.transform.position - start;
            float t = Vector3.Dot(hitVec, lineDir) / lineDir.sqrMagnitude;
            
            caughtObjects.Add(new CaughtObject
            {
                Rb = rb,
                Trans = rb.transform, // Cache transform
                CaptureT = Mathf.Clamp01(t)
            });
            
            rb.linearVelocity *= 0.5f; 
        }

        public void FlingAllObjects()
        {
            // Optimization: Cache standard positions outside loop
            Vector3 start = trailData.FromT.TransformPoint(trailData.FromLocalPos);
            Vector3 end = trailData.ToT.TransformPoint(trailData.ToLocalPos);

            foreach (var obj in caughtObjects)
            {
                if (obj.Rb)
                {
                    Vector3 anchor = Vector3.Lerp(start, end, obj.CaptureT);
                    Vector2 dir = (obj.Trans.position - anchor).normalized;
                    obj.Rb.AddForce(dir * (stiffness * 2f), ForceMode2D.Impulse);
                }
            }
            caughtObjects.Clear();
        }

        public void BreakConnection()
        {
            if (isBreaking) return;
            FlingAllObjects(); 
            StartCoroutine(AnimateSnap());
        }
        public void BreakConnectionNoFling()
        {
            if (isBreaking) return;
            StartCoroutine(AnimateSnap());
        }

        private IEnumerator AnimateSnap()
        {
            isBreaking = true;
            trailData.EdgeCol.enabled = false;
            trailData.Lr.enabled = false;

            Vector3 startPos = trailData.FromT.TransformPoint(trailData.FromLocalPos);
            Vector3 endPos = trailData.ToT.TransformPoint(trailData.ToLocalPos);
            Vector3 midPoint = (startPos + endPos) * 0.5f;
            
            // Create parts
            LineRenderer lineA = CreateBrokenLinePart("CheesePartA");
            LineRenderer lineB = CreateBrokenLinePart("CheesePartB");
            
            // Optimization: Cache positions for loop
            Vector3[] tempPositions = new Vector3[2]; 

            float timer = 0f;
            while (timer < tearDuration)
            {
                timer += Time.deltaTime;
                float t = timer / tearDuration; 
                float retractionT = t * t; 

                Vector3 currentEndA = Vector3.Lerp(midPoint, startPos, retractionT);
                Vector3 currentEndB = Vector3.Lerp(midPoint, endPos, retractionT);
                
                float snapWidth = Mathf.Lerp(trailData.Lr.widthMultiplier, 0f, t);
                lineA.widthMultiplier = snapWidth;
                lineB.widthMultiplier = snapWidth;

                // Set positions without allocating new arrays
                lineA.SetPosition(0, startPos); lineA.SetPosition(1, currentEndA);
                lineB.SetPosition(0, endPos); lineB.SetPosition(1, currentEndB);

                yield return null;
            }

            if(lineA) Destroy(lineA.gameObject);
            if(lineB) Destroy(lineB.gameObject);
            Destroy(gameObject);
        }
        
        // ReSharper disable Unity.PerformanceAnalysis
        private LineRenderer CreateBrokenLinePart(string name)
        {
            GameObject go = new GameObject(name);
            go.transform.position = Vector3.zero; 
            LineRenderer newLr = go.AddComponent<LineRenderer>();
            LineRenderer original = trailData.Lr;
            newLr.material = original.material;
            newLr.widthCurve = original.widthCurve;
            newLr.widthMultiplier = original.widthMultiplier;
            newLr.colorGradient = original.colorGradient;
            newLr.useWorldSpace = true;
            return newLr;
        }

        public static TrailSegment CreateSegment(GameObject linePrefab, Transform parent, GameObject fromGo, GameObject toGo)
        {
            var lineObject = Instantiate(linePrefab,parent);
            lineObject.name = "SlingshotSeg";
            lineObject.transform.position = Vector3.zero; // Keeps Local Space == World Space

            var lr = lineObject.GetComponent<LineRenderer>();
            if (lr == null) lr = lineObject.AddComponent<LineRenderer>();
            
            var edge = lineObject.GetComponentInChildren<EdgeCollider2D>();
            if (edge == null) edge = lineObject.AddComponent<EdgeCollider2D>();
            edge.isTrigger = true; 
            edge.edgeRadius = 0.2f;

            lr.useWorldSpace = true;

            var fromT = fromGo.transform;
            var toT = toGo.transform;

            var seg = lineObject.GetComponent<Segment>();
            seg.trailData = new TrailSegment
            {
                Lr = lr,
                EdgeCol = edge,
                FromT = fromT,
                ToT = toT,
                FromLocalPos = fromT.InverseTransformPoint(fromT.position),
                ToLocalPos = toT.InverseTransformPoint(toT.position),
                Seg = seg
            };
            return seg.trailData;
        }

        private void OnBrickShot()
        {
            Invoke(nameof(BreakConnectionNoFling), .3f);
        }

        private void OnResetButtonPressed()
        {
            isBreaking = false;
            BreakConnection();
        }
    }
}