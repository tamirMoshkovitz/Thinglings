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
    }
}