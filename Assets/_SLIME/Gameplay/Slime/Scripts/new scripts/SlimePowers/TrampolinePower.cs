using System;
using _SLIME.Gameplay.Slime.Scripts.new_scripts.SlimePowers;
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
        private bool isActive = false;
        public TrampolinePower(TrampolinePowerSettings trampolinePowerSettings)
        {
            _trampolinePowerSettings = trampolinePowerSettings;
        }


        public void Activate(Vector2 hitpoint,Collider2D objectToJump)
        {
            isActive = true;
            Rigidbody2D _projectileRb = objectToJump.GetComponent<Rigidbody2D>();
            GameObject _projectileGO = _projectileRb.gameObject;
            
            _projectileGO.layer = (int)Mathf.Log(_trampolinePowerSettings.slimeProjectileLayer.value, 2);
            
            _projectileRb.bodyType = RigidbodyType2D.Dynamic; 
            _projectileRb.gravityScale = 0;
            Vector2 direction = (objectToJump.transform.position - (Vector3)hitpoint).normalized;


            _projectileRb.linearVelocity = Vector2.zero;
            
            float power = _trampolinePowerSettings.trampolinePower; 
            _projectileRb.AddForce(direction * power, ForceMode2D.Impulse);
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