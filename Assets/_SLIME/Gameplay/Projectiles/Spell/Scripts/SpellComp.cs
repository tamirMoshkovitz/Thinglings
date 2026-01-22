using System;
using UnityEngine;

namespace _SLIME.Projectiles
{
    [Serializable]
    public struct SpellComp

    {
        public Rigidbody2D rb;
        public Animator animator;
        public Collider2D collider;
        public Transform[] spellHit;
    }
}