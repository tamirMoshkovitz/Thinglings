using System;
using _SLIME.Gameplay.Slime.Scripts.new_scripts.SlimePowers;
using Player.Brick;
using Unity.VisualScripting;
using UnityEngine;

namespace _SLIME.Gameplay.Slime.Scripts.new_scripts
{
    
    
    [System.Serializable]
    public struct TrampolinePowerSettings
    {
        public LayerMask slimeProjectileLayer;
        public float trampolinePower;
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
            Rigidbody2D projectileRb = objectToJump.GetComponent<Rigidbody2D>();
            GameObject projectileGo = projectileRb.gameObject;
            
            projectileGo.layer = (int)Mathf.Log(_trampolinePowerSettings.slimeProjectileLayer.value, 2);
            
            projectileRb.bodyType = RigidbodyType2D.Dynamic; 
            // _projectileRb.gravityScale = 0;
            Vector2 direction = (objectToJump.transform.position - (Vector3)hitpoint).normalized;


            projectileRb.linearVelocity = Vector2.zero;
            
            float power = _trampolinePowerSettings.trampolinePower;
            float stretchForce = _slimeData.StretchRatio * .75f + .25f;
            projectileRb.AddForce(direction * power * stretchForce, ForceMode2D.Impulse);

            if (projectileGo.TryGetComponent(typeof(BrickBehavior), out Component brickBehavior))
            {
                ((BrickBehavior)brickBehavior).Shoot(stretchForce);
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