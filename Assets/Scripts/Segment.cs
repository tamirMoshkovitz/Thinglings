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
            // public BoxCollider2D BoxCollider;
        }
        

        private TrailSegment trailData;
        private bool isBreaking = false;

        


        // [InspectorButton]
        // private void UpdateBoxCollider()
        // {
        //     if (trailData?.FromT == null || trailData?.ToT == null)
        //     {
        //         Debug.LogWarning("Cannot update box collider: FromT or ToT is null.");
        //         return;
        //     }
        //
        //     Vector2 fromPos = trailData.FromT.position;
        //     Vector2 toPos = trailData.ToT.position;
        //     Vector2 direction = (toPos - fromPos).normalized;
        //     float distance = Vector2.Distance(fromPos, toPos);
        //
        //     trailData.BoxCollider.size = new Vector2(distance, trailData.Lr.endWidth);
        //     trailData.BoxCollider.transform.position = (fromPos + toPos) * 0.5f;
        //     trailData.BoxCollider.offset = Vector2.zero;
        //
        //     float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        //     trailData.BoxCollider.transform.rotation = Quaternion.Euler(0, 0, angle);
        // }


        public void LateUpdate()
        {
            UpdatePositionAndVisuals();
            // UpdateBoxCollider();
        }
        
        
        public void BreakConnection()
        {
            if (isBreaking) return; 
            StartCoroutine(AnimateTear());
        }
        
        private IEnumerator AnimateTear()
        {
            isBreaking = true;
            float timer = 0f;

            
            Gradient originalGradient = trailData.Lr.colorGradient; 
            Color baseColor = originalGradient.colorKeys[0].color; 

            while (timer < tearDuration)
            {
                timer += Time.deltaTime;
                float progress = timer / tearDuration; // 0 עד 1

                
                Gradient gradient = new Gradient();

                
                gradient.SetKeys(
                    new GradientColorKey[] { new GradientColorKey(baseColor, 0.0f), new GradientColorKey(baseColor, 1.0f) },
                    
                    
                    new GradientAlphaKey[] 
                    {
                        new GradientAlphaKey(1.0f, 0.0f), // התחלה (נראה)
                        new GradientAlphaKey(1.0f, 0.5f - (progress * 0.5f)), // עד איפה רואים מצד שמאל
                        new GradientAlphaKey(0.0f, 0.5f - (progress * 0.5f) + 0.01f), // תחילת החור השקוף
                        new GradientAlphaKey(0.0f, 0.5f + (progress * 0.5f) - 0.01f), // סוף החור השקוף
                        new GradientAlphaKey(1.0f, 0.5f + (progress * 0.5f)), // חזרה לראות מצד ימין
                        new GradientAlphaKey(1.0f, 1.0f)  // סוף (נראה)
                    }
                );

                trailData.Lr.colorGradient = gradient;
                
                waveHeight += Time.deltaTime * 2.0f; 

                yield return null;
            }

            
            Destroy(gameObject);
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
            
            trailData.Lr.positionCount = pointsCount;
            
            for (int i = 0; i < pointsCount; i++)
            {
              
                float t = (float)i / (pointsCount - 1);

                
                Vector3 basePosition = Vector3.Lerp(startPos, endPos, t);

                
                float waveOffset = Mathf.Sin(t * waveFrequency + Time.time * waveSpeed) * waveHeight;
                float noise = Mathf.PerlinNoise(t * 10f, Time.time * waveSpeed) * waveHeight * 0.5f;
                
               
                float maskEdge = Mathf.Sin(t * Mathf.PI);

             
                Vector3 liquidOffset = new Vector3(0, (waveOffset + noise) * maskEdge, 0);

                
                trailData.Lr.SetPosition(i, basePosition + liquidOffset);
            }
            
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
            // var box = lineObject.GetComponentInChildren<BoxCollider2D>();
            // if (box == null)
            //     Debug.LogError(new Exception("No BoxCollider2D component attached to a child."));
            // box.offset = Vector2.zero;
            // box.size = Vector2.zero;
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
                Seg = seg
                // BoxCollider = box,
            };

            return seg.trailData;
        }

    }
}