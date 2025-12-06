using UnityEngine;

public class EyeFollow : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform pupil; 
    [SerializeField] private Transform player; 

    [Header("Settings")]
    public float eyeRadius = 0.5f;

    void Update()
    {
        Vector3 direction = player.position - transform.position;

        Vector3 clampedOffset = Vector3.ClampMagnitude(direction, eyeRadius);

        pupil.localPosition = clampedOffset;
    }
}