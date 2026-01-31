using System;
using NaughtyAttributes;
using UnityEngine;

namespace _SLIME.LightHouse
{
    [Serializable]
    public struct LightHouseSets
    {
        [MinMaxSlider(0f, 7f)] public Vector2 attackDuration;
        [MinMaxSlider(0f, 360f)] public Vector2 mainBeamSpeedPerSecond;
        [MinMaxSlider(0f, 360f)] public Vector2 otherBeamsSpeedPerSecond;
        [MinMaxSlider(0f, 360f)] public Vector2 angleFromMainBeam;
        [MinMaxSlider(0f, 360f)] public Vector2 mainBeamAngleFromSlimes;
        public float minBeamAngleDistance;

        public float timeToCheckForMainBeanSwitch;
        
    }
}