
using _SLIME.BaseScripts;
using _SLIME.Projectiles;
using _SLIME.Projectiles;

using UnityEngine;

namespace _SLIME.Slime
{
    
    
    [System.Serializable]
    public struct SparkPowerSettings
    {
        public LayerMask slimeProjectileLayer;
        public float sparkRadius;
        public float sparkDamage;
        public LayerMask enemyHitLayer;
    }
    
    [System.Serializable]
    public struct SparkPowerDep
    {
        public GameObject sparkShader;
        public GameObject slime1;
        public GameObject slime2;
    }

    public class SparkPower: ISlimePower
    {
        private readonly SparkPowerSettings _sparkPowerSettings;
        private readonly SlimeData _slimeData;
        private bool isActive = false;
        private readonly SparkPowerDep _sparkPowerDeps;

        public SparkPower(SparkPowerSettings sparkPowerSettings, 
            SparkPowerDep sparkPowerDep, SlimeData slimeData)
        {
            _sparkPowerSettings = sparkPowerSettings;
            _sparkPowerDeps = sparkPowerDep;
            _slimeData = slimeData;
        }


        public void Activate(Vector2 hitpoint,Collider2D collider)
        {
            SlimeEvents.SparkActivated?.Invoke();
            
            Vector3 slime1Position = _sparkPowerDeps.slime1.transform.position;
            Vector3 slime2Position = _sparkPowerDeps.slime2.transform.position;
            Vector3 middlePosition = (slime1Position + slime2Position) / 2f;
            
            // Collider2D[] hitColliders = Physics2D.OverlapCircleAll(middlePosition, _sparkPowerSettings.sparkRadius, _sparkPowerSettings.enemyHitLayer);
            // foreach (var hitCollider in hitColliders)
            // {
            //     var rig = hitCollider.attachedRigidbody;
            //     if (rig && rig.TryGetComponent<IHealth>(out IHealth health))
            //     {
            //         health.TakeDamage(_sparkPowerSettings.sparkDamage);
            //     }
            // }
            
            Object.Instantiate(_sparkPowerDeps.sparkShader, middlePosition, Quaternion.identity);
            isActive = true;
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

