using System;
using _SLIME.BaseScripts;
using _SLIME.Boss;
using _SLIME.Core.Audio.FMOD.Scripts;
using _SLIME.GameLoop;
using FMODUnity;
using UnityEngine;
using NaughtyAttributes;
using UnityEngine.Serialization;

namespace _SLIME.Projectiles
{
    public enum SpellState { Spawning, Flying, Hit }
    
    public class Spell : ProjectMonoBehavior
    {
        private static readonly int Hit = Animator.StringToHash("Hit");
        private static readonly int BossMove = Animator.StringToHash("BossMove");
        private static readonly int PlayerMove = Animator.StringToHash("SlimeMove");

        [SerializeField] private SpellComp comp;

        [SerializeField]
        private BaseBossConfigurations[] bossConfigurations;
        private SpellBossAttributes _bossAttributes;
        private SpellState _currentState;
        
        [MinMaxSlider(0.1f, 10f)]
        [SerializeField] private Vector2 spellScaleFactor;

        [Header("SFX")] 
        [SerializeField] private EventReference spawnSfX;
        [SerializeField] private EventReference shootSFX;

        [Header("Deflect Cooldown")]
        [SerializeField] private float deflectCooldownDuration = 0.5f;
        private float _lastDeflectTime = float.MinValue;
        
    
        private float _lastHitTime = float.MinValue;

        private void OnEnable()
        {
            SFXPlayer.Play(spawnSfX);
        }

        public void BossSetup(SpellBossAttributes attributes)
        {
            
            _bossAttributes = attributes;
            comp.rb.linearVelocity = Vector2.zero;
            comp.rb.bodyType = RigidbodyType2D.Kinematic;
            comp.spellHead.transform.rotation = Quaternion.Euler(0f, 0f, attributes.z);

        }
        
        public void Deflect(SpellSlimeAttributes attributes)
        {
            if(_currentState != SpellState.Flying) return;
            float currentTime = Time.time;
            if (currentTime - _lastDeflectTime < deflectCooldownDuration)
                return;
            // If a hit was just handled in the last 0.05s, don't allow a deflect
            if (currentTime - _lastHitTime <= 0.001f) return;
            _lastDeflectTime = currentTime;
            
            comp.collider.gameObject.layer = GetLayerFromMask(attributes.layerMask);
            comp.rb.bodyType = RigidbodyType2D.Dynamic; 
            float incomingSpeed = comp.rb.linearVelocity.magnitude;
            comp.rb.linearVelocity = Vector2.zero;
            float finalPower = incomingSpeed * attributes.deflectionPower;
            comp.rb.AddForce(attributes.direction *  finalPower, ForceMode2D.Impulse);
            comp.animator.SetTrigger(PlayerMove);

            // Mark this frame so we can ignore collision hits in the same frame.
           
            
            SFXPlayer.Play(shootSFX);

         
        }
        
        private void Shoot()
        {
            _currentState = SpellState.Flying;
            comp.rb.linearVelocity = _bossAttributes.direction.normalized
                                     * _bossAttributes.moveSpeed;
            comp.animator.SetTrigger(BossMove);
            // SFXPlayer.Play(spellShootSFX); boss Shooting
        }

       
        public void OnSpawnFinished()
        {
            if (_currentState != SpellState.Spawning) return;
            Shoot();
        }
        
        public void OnHitFinished()
        {
            Destroy(transform.parent.gameObject);
        }
        
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (_currentState != SpellState.Flying) return;
            
            var rig = other.attachedRigidbody;
            if (rig && rig.TryGetComponent<IHealth>(out IHealth h))
            {
                HandleImpact(h);
            }
            else if (other.CompareTag("Wall")) 
            {
                HandleImpact(null); 
            }
        }

        private void HandleImpact(IHealth target)
        {
            // If a deflect was just done in the last 0.05s, ignore this hit
            if (Time.time - _lastDeflectTime <= 0.001f) return;
            _lastHitTime = Time.time;
            _currentState = SpellState.Hit;
            var currentSpeed = comp.rb.linearVelocity.magnitude;
            comp.rb.linearVelocity = Vector2.zero;
            comp.rb.bodyType = RigidbodyType2D.Kinematic;
            if (target != null)
            {
                
                target.TakeDamage(Mathf.CeilToInt(currentSpeed));
            }
            HitParticleScaleChange(currentSpeed);
            comp.animator.SetTrigger(Hit);
        }

        private void HitParticleScaleChange(float currentSpeed)
        {
            
            float expectedSpeed = BossBrain.bossConfigurations ? BossBrain.bossConfigurations.PhaseSettings.expectedAvgSpeedOfSpells
                    : currentSpeed;
            float minScale = spellScaleFactor.x;
            float maxScale = spellScaleFactor.y;
            float midScale = (minScale + maxScale) * 0.5f;
            
            float speedRatio = currentSpeed / expectedSpeed;
            
            float scale;
            if (speedRatio <= 1f)
            {
                scale = Mathf.Lerp(minScale, midScale, speedRatio);
            }
            else
            {
                float t = (speedRatio - 1f);
                scale = Mathf.Lerp(midScale, maxScale, t);
            }
            
            scale = Mathf.Clamp(scale, minScale, maxScale);
            
            foreach (var h in comp.spellHit)
            {
                h.localScale = Vector3.one * scale;
            }
        }


        private int GetLayerFromMask(LayerMask mask)
        {
            int layerNumber = 0;
            int layer = mask.value;
            while (layer > 1)
            {
                layer >>= 1;
                layerNumber++;
            }
            return layerNumber;
        }
    }
}