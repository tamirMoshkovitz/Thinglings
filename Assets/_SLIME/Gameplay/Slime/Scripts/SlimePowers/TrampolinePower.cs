
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
        [Tooltip("Scale-down curve when deflected. Duration = scaleDownDurationFactor / finalPower.")]
        public AnimationCurve spellScaleDownCurve;
        public float scaleDownDurationFactor;
        [Tooltip("Target scale when deflected (e.g. Vector3.zero).")]
        public Vector3 spellScaleDownTarget;
        [Tooltip("Deflect lob: arc height. 0 = no lob.")]
        public float deflectLobArcHeight;
        [Tooltip("Deflect lob: upward phase duration (collider off).")]
        public float deflectLobUpDuration;
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
            isActive = true;
            if (objectToJump.gameObject.CompareTag("Icicle"))
            {
                SlimeEvents.SlimeConnectionGotHitByIcicle?.Invoke();
                return;
            }

            Rigidbody2D projectileRb = objectToJump.attachedRigidbody;
            GameObject projectileGo = projectileRb.gameObject;
            
            Vector2 direction = (objectToJump.transform.position - (Vector3)hitpoint).normalized;
            
            float stretchForce = _trampolinePowerSettings.trampolinePowerCurve.Evaluate(_slimeData.StretchRatio);
            float power = Mathf.Lerp(_trampolinePowerSettings.deflectionPower.x,
             _trampolinePowerSettings.deflectionPower.y, stretchForce);
            
            if (projectileGo.TryGetComponent(typeof(Spell), out Component spell))
            {
                ((Spell)spell).Deflect(new SpellSlimeAttributes
                {
                    deflectionPower = power,
                    direction = direction,
                    layerMask = _trampolinePowerSettings.slimeProjectileLayer,
                    scaleDownCurve = _trampolinePowerSettings.spellScaleDownCurve,
                    scaleDownDurationFactor = _trampolinePowerSettings.scaleDownDurationFactor,
                    scaleDownTarget = _trampolinePowerSettings.spellScaleDownTarget,
                    deflectLobArcHeight = _trampolinePowerSettings.deflectLobArcHeight,
                    deflectLobUpDuration = _trampolinePowerSettings.deflectLobUpDuration
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