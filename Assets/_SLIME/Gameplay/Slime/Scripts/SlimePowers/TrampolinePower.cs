
using _SLIME.Projectiles;
using NaughtyAttributes;

using UnityEngine;

namespace _SLIME.Slime
{
    
    
    [System.Serializable]
    public struct TrampolinePowerSettings
    {
        public LayerMask slimeProjectileLayer;
        [MinMaxSlider(0f,70f)]
        public Vector2 deflectionPower;
        public AnimationCurve trampolinePowerCurve;
        
    }
    
    public class TrampolinePower: ISlimePower
    {
        private readonly TrampolinePowerSettings _trampolinePowerSettings;
        private readonly SlimeData _slimeData;
        private bool isActive = false;
        public TrampolinePower(TrampolinePowerSettings trampolinePowerSettings, SlimeData slimeData)
        {
            _trampolinePowerSettings = trampolinePowerSettings;
            _slimeData = slimeData;
        }


        // ReSharper disable Unity.PerformanceAnalysis
        public void Activate(Vector2 hitpoint,Collider2D objectToJump)
        {
            if (objectToJump.gameObject.layer == LayerMask.NameToLayer("Obstacles"))
            {
                return;
            }
            if (objectToJump.gameObject.CompareTag("Icicle"))
            {
                SlimeEvents.SlimeConnectionGotHitByIcicle?.Invoke();
                return;
            }
            isActive = true;

            Rigidbody2D projectileRb = objectToJump.attachedRigidbody;
            GameObject projectileGo = projectileRb.gameObject;

            Vector2 dirFromHit = ((Vector2)objectToJump.transform.position - hitpoint).normalized;
            Vector2 vel = projectileRb.linearVelocity;
            Vector2 direction;
            if (vel.sqrMagnitude > 0.01f)
            {
                Vector2 dirFromVel = -vel.normalized;
                direction = Vector2.Dot(dirFromHit, dirFromVel) < 0f ? dirFromVel : dirFromHit;
            }
            else
            {
                direction = dirFromHit;
            }
            
            float stretchForce = _trampolinePowerSettings.trampolinePowerCurve.Evaluate(_slimeData.StretchRatio);
            float power = Mathf.Lerp(_trampolinePowerSettings.deflectionPower.x,
             _trampolinePowerSettings.deflectionPower.y, stretchForce);
            
            if (projectileGo.TryGetComponent(typeof(Spell), out Component spell))
            {
                ((Spell)spell).Deflect(new SpellSlimeAttributes
                {
                    deflectionPower = power,
                    direction = direction,
                    layerMask = _trampolinePowerSettings.slimeProjectileLayer
                });
            }
            SlimeEvents.TrampolineActivated();
        }

        public void Update()
        {
           if(!isActive) return;
        }

        public void Deactivate()
        {
           
        }
    }
}