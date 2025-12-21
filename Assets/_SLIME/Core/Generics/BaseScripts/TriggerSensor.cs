using System;
using UnityEngine;


namespace _SLIME.BaseScripts
{
    [RequireComponent(typeof(Collider2D))]
    public class TriggerSensor : ProjectMonoBehavior
    {
        [SerializeField] private Collider2D collider;
        public event Action<Vector2,Collider2D> OnTriggerEntered;
        public event Action<Collider2D> OnTriggerExited; 
        private void OnTriggerEnter2D(Collider2D other)
        {
            Vector2 hitPoint = collider.ClosestPoint(other.transform.position);
            OnTriggerEntered?.Invoke(hitPoint,other);
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            OnTriggerExited?.Invoke(other);
        }
    }
}