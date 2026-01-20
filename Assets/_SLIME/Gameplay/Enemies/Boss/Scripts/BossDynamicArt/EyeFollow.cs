using _SLIME.BaseScripts;
using UnityEngine;


namespace _SLIME.Boss
{
    public class EyeFollow : ProjectMonoBehavior
    {
        [Header("References")] [SerializeField]
        private Transform pupil;

        [SerializeField] private Transform player;

        [Header("Settings")] public float eyeRadius = 0.1f;
        public float trackingSensitivity = 0.5f;

        void Update()
        {
            if (!player || !pupil) return;
            Vector3 localTarget = transform.InverseTransformPoint(player.position);
            localTarget.z = 0;
            float actualDistance = localTarget.magnitude;
            Vector3 direction = localTarget.normalized;
            float movementPercent = Mathf.Clamp01(actualDistance / trackingSensitivity);
            pupil.localPosition = direction * (movementPercent * eyeRadius);
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, eyeRadius);
            Gizmos.color = new Color(1, 0, 0, 0.2f);
            Gizmos.DrawWireSphere(transform.position, trackingSensitivity);
        }
    }
}