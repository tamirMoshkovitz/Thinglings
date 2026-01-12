
using _SLIME.Gameplay.Projectiles.Brick.Scripts;
using _SLIME.Projectiles;

using UnityEngine;

namespace _SLIME.Slime
{
    
    
    [System.Serializable]
    public struct TrampolinePowerSettings
    {
        public LayerMask slimeProjectileLayer;
        public float trampolineMinPower;
        public float trampolineMaxPower;
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


        public void Activate(Vector2 hitpoint,Collider2D objectToJump)
        {
            isActive = true;
            if (objectToJump.gameObject.CompareTag("Icicle"))
            {
                SlimeEvents.SlimeConnectionGotHitByIcicle?.Invoke();
                return;
            } 
            Rigidbody2D projectileRb = objectToJump.GetComponent<Rigidbody2D>();
            GameObject projectileGo = projectileRb.gameObject;
            
            projectileGo.layer = (int)Mathf.Log(_trampolinePowerSettings.slimeProjectileLayer.value, 2);
            
            projectileRb.bodyType = RigidbodyType2D.Dynamic; 
            Vector2 direction = (objectToJump.transform.position - (Vector3)hitpoint).normalized;


            projectileRb.linearVelocity = Vector2.zero;
            
            float stretchForce = _trampolinePowerSettings.trampolinePowerCurve.Evaluate(_slimeData.StretchRatio);
            float power = Mathf.Lerp(_trampolinePowerSettings.trampolineMinPower,
             _trampolinePowerSettings.trampolineMaxPower, stretchForce);
            projectileRb.AddForce(direction * power, ForceMode2D.Impulse);

            if (projectileGo.TryGetComponent(typeof(Spell), out Component spell))
            {
                ((Spell)spell).Shoot(stretchForce);
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