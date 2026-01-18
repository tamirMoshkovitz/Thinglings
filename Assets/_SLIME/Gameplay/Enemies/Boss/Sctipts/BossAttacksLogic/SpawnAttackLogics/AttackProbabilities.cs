using System;
using UnityEngine;

namespace _SLIME.Boss
{
    [Serializable]
    public struct AttackProbabilities
    {
        [SerializeField] private float oneShot;
        [SerializeField] private float twoShots;
        [SerializeField] private float threeShot;
        [SerializeField] private float fourShots;
        [SerializeField] private float bulletHell;

        public float OneShot => oneShot;
        public float TwoShots => twoShots;
        public float ThreeShot => threeShot;
        public float FourShots => fourShots;
        public float BulletHell => bulletHell;

        public float Total => oneShot + twoShots + threeShot + fourShots + bulletHell;

        public void Normalize()
        {
            float total = Total;
            if (total <= 0f)
            {
                oneShot = 1f;
                twoShots = 0f;
                threeShot = 0f;
                fourShots = 0f;
                bulletHell = 0f;
                return;
            }

            oneShot /= total;
            twoShots /= total;
            threeShot /= total;
            fourShots /= total;
            bulletHell /= total;
        }

        public PossibleAttacks GetRandomAttack()
        {
            float random = UnityEngine.Random.value;
            float cumulative = 0f;

            cumulative += oneShot;
            if (random < cumulative) return PossibleAttacks.OneShot;

            cumulative += twoShots;
            if (random < cumulative) return PossibleAttacks.TwoShots;

            cumulative += threeShot;
            if (random < cumulative) return PossibleAttacks.ThreeShot;

            cumulative += fourShots;
            if (random < cumulative) return PossibleAttacks.FourShots;

            return PossibleAttacks.BulletHell;
        }
    }
}
