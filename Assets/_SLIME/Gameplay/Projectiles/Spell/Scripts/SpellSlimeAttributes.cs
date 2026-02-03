using System;
using UnityEngine;

namespace _SLIME.Projectiles
{

    [Serializable]
    public struct SpellSlimeAttributes
    {
        public float deflectionPower;
        public Vector2 direction;
        public LayerMask layerMask;
        [Tooltip("Scale-down curve (0->1, 1->0). Duration = scaleDownDurationFactor / finalPower.")]
        public AnimationCurve scaleDownCurve;
        public float scaleDownDurationFactor;
        [Tooltip("Target scale when deflected (e.g. Vector3.zero to shrink away).")]
        public Vector3 scaleDownTarget;
        [Tooltip("Lob arc height when deflected (up then down). 0 = no lob.")]
        public float deflectLobArcHeight;
        [Tooltip("Duration of upward phase (collider off during this).")]
        public float deflectLobUpDuration;
    }
}