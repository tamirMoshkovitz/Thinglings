using System;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Player
{
    public class Segment : MonoBehaviour
    {
        
        [Header("Dynamic Thickness Settings")]
        public float maxThickness = 0.5f;    
        public float minThickness = 0.05f;   
        public float closeDistance = 1.0f;
        public float farDistance = 10.0f;

        [Header("Liquid Wave Settings")]
        public int pointsCount = 50;         
        public float waveHeight = 0.2f;      
        public float waveSpeed = 5.0f;       
        public float waveFrequency = 10.0f;
        
        [Header("Tear Effect Settings")]
        public float tearDuration = 0.5f;
        
        public class TrailSegment
        {
            public LineRenderer Lr;
            public Transform FromT, ToT;
            public Vector3 FromLocalPos, ToLocalPos;

            public Segment Seg;
            public BoxCollider2D BoxCollider;
        }
        

        private TrailSegment trailData;
        private bool isBreaking = false;

        


        // [InspectorButton]
        private void UpdateBoxCollider()
        {
            if (trailData?.FromT == null || trailData?.ToT == null)
            {
                Debug.LogWarning("Cannot update box collider: FromT or ToT is null.");
                return;
            }
        
            Vector2 fromPos = trailData.FromT.position;
            Vector2 toPos = trailData.ToT.position;
            Vector2 direction = (toPos - fromPos).normalized;
            float distance = Vector2.Distance(fromPos, toPos);
        
            trailData.BoxCollider.size = new Vector2(distance, trailData.Lr.endWidth);
            trailData.BoxCollider.transform.position = (fromPos + toPos) * 0.5f;
            trailData.BoxCollider.offset = Vector2.zero;
        
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            trailData.BoxCollider.transform.rotation = Quaternion.Euler(0, 0, angle);
        }


        public void LateUpdate()
        {
            UpdatePositionAndVisuals();
            UpdateBoxCollider();
        }
        
        
        public void BreakConnection()
        {
            if (isBreaking) return; 
            StartCoroutine(AnimateTear());
        }
        
        private void DrawWaveOnLine(LineRenderer lr, Vector3 p1, Vector3 p2, float currentWaveHeight)
        {
          
            float dist = Vector3.Distance(p1, p2);
            
            
            lr.positionCount = pointsCount; 
            
            for (int i = 0; i < pointsCount; i++)
            {
                float t = (float)i / (pointsCount - 1);
                Vector3 basePosition = Vector3.Lerp(p1, p2, t);

                float waveOffset = Mathf.Sin(t * waveFrequency + Time.time * waveSpeed) * currentWaveHeight;
                float noise = Mathf.PerlinNoise(t * 10f, Time.time * waveSpeed) * currentWaveHeight * 0.5f;
                float maskEdge = Mathf.Sin(t * Mathf.PI);

                
                Vector3 liquidOffset = new Vector3(0, (waveOffset + noise) * maskEdge, 0);
                lr.SetPosition(i, basePosition + liquidOffset);
            }
        }
        
        
        private void UpdatePositionAndVisuals()
        {
            Vector3 startPos = trailData.FromT.TransformPoint(trailData.FromLocalPos);
            Vector3 endPos = trailData.ToT.TransformPoint(trailData.ToLocalPos);
            
            float distance = Vector3.Distance(startPos, endPos);
            float tWidth = Mathf.InverseLerp(closeDistance, farDistance, distance);
            float currentWidth = Mathf.Lerp(maxThickness, minThickness, tWidth);
            
            trailData.Lr.startWidth = currentWidth;
            trailData.Lr.endWidth = currentWidth;

            
            DrawWaveOnLine(trailData.Lr, startPos, endPos, waveHeight);
        }

        
        private IEnumerator AnimateTear()
        {
            isBreaking = true;
            if (trailData.BoxCollider != null) trailData.BoxCollider.enabled = false;

            Vector3 startPos = trailData.FromT.TransformPoint(trailData.FromLocalPos);
            Vector3 endPos = trailData.ToT.TransformPoint(trailData.ToLocalPos);
            Vector3 midPoint = (startPos + endPos) * 0.5f;

            
            LineRenderer lineA = CreateBrokenLinePart("LinePartA");
            LineRenderer lineB = CreateBrokenLinePart("LinePartB");

            trailData.Lr.enabled = false; // הסתרת הקו המקורי

            float timer = 0f;

            while (timer < tearDuration)
            {
                timer += Time.deltaTime;
                float progress = timer / tearDuration; 
                float easeProgress = progress * progress; // תנועה מואצת

               
                Vector3 currentEndA = Vector3.Lerp(midPoint, startPos, easeProgress);
                Vector3 currentEndB = Vector3.Lerp(midPoint, endPos, easeProgress);

                
                float currentWaveHeight = waveHeight * (1 + progress);
                
                DrawWaveOnLine(lineA, startPos, currentEndA, currentWaveHeight);
                DrawWaveOnLine(lineB, endPos, currentEndB, currentWaveHeight); 

                yield return null;
            }

            if(lineA != null) Destroy(lineA.gameObject);
            if(lineB != null) Destroy(lineB.gameObject);
            Destroy(gameObject);
        }

        private LineRenderer CreateBrokenLinePart(string name)
        {
            GameObject go = new GameObject(name);
            // חשוב: שים אותו תחת אותו הורה אם צריך, או בעולם
            go.transform.position = Vector3.zero; 
            
            LineRenderer newLr = go.AddComponent<LineRenderer>();
            LineRenderer original = trailData.Lr;
            
            newLr.material = original.material;
            newLr.widthMultiplier = original.widthMultiplier;
            newLr.startWidth = original.startWidth;
            newLr.endWidth = original.endWidth;
            newLr.colorGradient = original.colorGradient;
            newLr.numCapVertices = 60; 
            newLr.numCornerVertices = 5;
            newLr.alignment = original.alignment;
            newLr.useWorldSpace = true;

            return newLr;
        }
        public static TrailSegment CreateSegment(GameObject linePrefab, Transform parent, GameObject fromGo,
            GameObject toGo)
        {
            // Instantiate the prefab and parent it
            var lineObject = Instantiate(linePrefab);
            lineObject.name = fromGo.name + " TO " + toGo.name;

            // Optionally reset local transform
            lineObject.transform.localPosition = Vector3.zero;
            lineObject.transform.localRotation = Quaternion.identity;
            lineObject.transform.localScale = Vector3.one;

            // Ensure LineRenderer
            var lr = lineObject.GetComponent<LineRenderer>();
            if (lr == null)
                lr = lineObject.AddComponent<LineRenderer>();
            var box = lineObject.GetComponentInChildren<BoxCollider2D>();
            if (box == null)
                Debug.LogError(new Exception("No BoxCollider2D component attached to a child."));
            box.offset = Vector2.zero;
            box.size = Vector2.zero;
            lr.useWorldSpace = true;
            lr.positionCount = 2;

            // Set positions
            var fromT = fromGo.transform;
            var toT = toGo.transform;
            Vector3 fromLocal = fromT.InverseTransformPoint(fromT.position);
            Vector3 toLocal = toT.InverseTransformPoint(toT.position);

            lr.SetPosition(0, fromT.position);
            lr.SetPosition(1, toT.position);
            var seg = lineObject.GetComponent<Segment>();
            seg.trailData = new TrailSegment
            {
                Lr = lr,
                FromT = fromT,
                ToT = toT,
                FromLocalPos = fromLocal,
                ToLocalPos = toLocal,
                Seg = seg,
                BoxCollider = box,
            };

            return seg.trailData;
        }

    }
}