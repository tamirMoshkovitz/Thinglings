using _SLIME.BaseScripts;
using UnityEngine;

namespace _SLIME.Gameplay.Projectiles.Brick.Scripts
{
    public class HarmfulPinballSpell: PinballSpell
    {
        [SerializeField] private int damage = 5;
        protected override void OnTriggerEnter2D(Collider2D collision)
        {
            var rig = collision.attachedRigidbody;
            if (rig && rig.TryGetComponent<IHealth>(out IHealth h))
            {
                h.TakeDamage(damage);
            }
            
            if (collision.gameObject.CompareTag("Player"))
            {
                InitiateFastDissolve();
                DisableColliders();
            }        
        }
    }
}