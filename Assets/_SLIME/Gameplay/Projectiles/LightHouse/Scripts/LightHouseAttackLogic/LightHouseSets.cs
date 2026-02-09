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
        [Tooltip("Multiplier (>1) applied to beam speeds by the end of the attack.")]
        [Min(1f)] public float beamSpeedMultiplier;
        [Tooltip("Acceleration curve over normalized attack time (0=start, 1=end). 0 -> base speed, 1 -> base*multiplier.")]
        public AnimationCurve beamSpeedCurve;
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