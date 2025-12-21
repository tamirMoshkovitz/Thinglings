using System;
using _SLIME.BaseScripts;
using UnityEngine;

namespace _SLIME.StartScene
{


    public class FluidParticalSystem : ProjectMonoBehavior
    {
        [SerializeField] private new ParticleSystem particleSystem;

        [Obsolete("Obsolete")]
        private void Update()
        {
            particleSystem.gravityModifier = Mathf.Sin(Time.time);
        }
    }
}
