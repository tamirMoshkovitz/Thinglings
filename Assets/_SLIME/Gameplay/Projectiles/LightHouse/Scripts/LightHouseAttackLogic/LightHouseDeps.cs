using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace _SLIME.LightHouse
{
    [Serializable]
    public struct LightHouseDeps
    {
        public List<Collider2D> colliders;
        public Transform lighthouseCenter;
        public Transform closeLaserRotationPoint;
        public Transform midLaserRotationPoint;
        public Transform farLaserRotationPoint;
        public Transform closeLaserHitPoint;
        public Transform midLaserHitPoint;
        public Transform farLaserHitPoint;
        public ControlledSfx lightHouseSfx;
    }
}