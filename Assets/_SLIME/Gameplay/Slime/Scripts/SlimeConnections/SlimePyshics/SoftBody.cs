using System.Collections;
using System.Collections.Generic;
using _SLIME.BaseScripts;
using UnityEngine;
using UnityEngine.U2D;

namespace _SLIME.Slime
{


    public class SoftBody : ProjectMonoBehavior
    {
        #region Fields

        [SerializeField] public SpriteShapeController spriteShape;

        [SerializeField] public Transform[] points;

        // Added this field, as it was used in your 'catch' block
        [SerializeField] private Vector2 splineOffset = new(0.1f, 0.1f);

        #endregion

        #region Monobehaviour Callbacks

        private void Awake()
        {
            UpdateVerticies();
        }

        private void Update()
        {
            UpdateVerticies();
        }

        #endregion

        #region privateMethods

        // ReSharper disable Unity.PerformanceAnalysis
        private void UpdateVerticies()
        {
            // Corrected 'points.length' to 'points.Length'
            for (int i = 0; i < points.Length - 1; i++)
            {
                Vector2 vertex = points[i].localPosition;

                Vector2 towardsCenter = (Vector2.zero - vertex).normalized;
                float colliderRadius = points[i].gameObject.GetComponent<CircleCollider2D>().radius;
                try
                {
                    spriteShape.spline.SetPosition(i, (vertex - towardsCenter * colliderRadius));
                }
                catch
                {
                    Debug.Log("Spline points are too close to each other.. recalculate");
                    spriteShape.spline.SetPosition(i, (vertex - towardsCenter * colliderRadius + splineOffset));
                }

                Vector2 lt = spriteShape.spline.GetLeftTangent(i);

                Vector2 newRt = Vector2.Perpendicular(towardsCenter) * lt.magnitude;
                Vector2 newLt = Vector2.zero - (newRt);

                spriteShape.spline.SetRightTangent(i, newRt);
                spriteShape.spline.SetLeftTangent(i, newLt);
            }
        }

        #endregion
    }
}