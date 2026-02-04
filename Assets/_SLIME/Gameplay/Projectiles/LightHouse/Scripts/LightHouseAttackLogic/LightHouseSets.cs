using System;
using NaughtyAttributes;
using UnityEngine;

namespace _SLIME.LightHouse
{
    [Serializable]
    public struct LightHouseSets
    {
        [MinMaxSlider(0f, 15f)] public Vector2 attackDuration;
        public float farBeamSpeed;
        public float midBeamSpeed;
        public float closeBeamSpeed;
        [MinMaxSlider(0f, 360f)] public Vector2 angleFromMainBeam;
        [MinMaxSlider(0f, 360f)] public Vector2 mainBeamAngleFromSlimes;
        public float minBeamAngleDistance;

        [Tooltip("Minimum seconds between direction flips; next flip only after this cooldown.")]
        public float timeToCheckForMainBeanSwitch;
        [Tooltip("Duration for deceleration before direction flip and acceleration after.")]
        public float directionFlipTransitionDuration;

        [Tooltip("Angle dead zone (deg) - don't flip direction when |beam - slime| is below this. Prevents oscillation.")]
        public float directionFlipDeadZone;

        public float floatBossDistance;
        public float floatBossDuration;
    }
}