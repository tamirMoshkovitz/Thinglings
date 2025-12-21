using _SLIME.BaseScripts;
using UnityEngine;

namespace _SLIME.Boss
{
    public class HandController : ProjectMonoBehavior
    {
        [Header("References")] public Transform player;
        public Transform centerBound;
        public Camera mainCamera;

        [Header("Position Settings")] public float hoverY = 20f;
        public float prepareY = 18f;
        public float floorY = -1.5f;

        [Header("Attack Settings")] public float entryDuration = 0.5f;
        public float trackDuration = 2.0f;
        public float lockDuration = 0.5f;
        public float trackSpeed = 10f;
        public float dropDuration = 0.2f;
        public float riseDuration = 1.0f;

        [Header("Camera Shake")] public float shakeDuration = 0.3f;
        public float shakeStrength = 0.3f;
        public int shakeVibrato = 10;

        [Header("Hand Side")] public bool isRightHand = false;

        private void Start()
        {
            if (mainCamera == null) mainCamera = Camera.main;
        }
    }
}