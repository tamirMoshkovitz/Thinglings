using System;
using UnityEngine;

public class FluidParticalSystem : MonoBehaviour
{
    [SerializeField] private new ParticleSystem particleSystem;

    [Obsolete("Obsolete")]
    private void Update()
    {
        particleSystem.gravityModifier = Mathf.Sin(Time.time);
    }
}
