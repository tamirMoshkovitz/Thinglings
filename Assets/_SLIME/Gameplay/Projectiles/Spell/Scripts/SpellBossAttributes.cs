using System;
using UnityEngine;

namespace _SLIME.Projectiles
{

    [Serializable]
    public struct SpellBossAttributes
    {
        public Vector3 spawnPosition;
        public Vector3 targetPosition;
        public float moveSpeed;
        public float z;
        public AnimationCurve scaleUpCurve;
        public float scaleUpDurationFactor;
        public Vector3 scaleStart;
        public float scaleUpCloseDistanceThreshold;
        public float scaleUpDurationWhenClose;
        [Tooltip("Lob arc: height above direct line. 0 = straight line.")]
        public float lobArcHeight;
    }
    
    
}