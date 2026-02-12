using System;
using UnityEngine;

namespace _SLIME.Gameplay.Projectiles.Icicle.Scripts
{
    public class DisableOffGameobjectAfterTime : MonoBehaviour
    {
        public void Start()
        {
            gameObject.SetActive(false);
        }
    }
}