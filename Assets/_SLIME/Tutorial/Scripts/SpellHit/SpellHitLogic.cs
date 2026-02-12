using System;
using System.Collections;
using _SLIME.Projectiles;
using _SLIME.Boss;
using DG.Tweening;
using UnityEngine;

namespace _SLIME.Tutorial
{
    [System.Serializable]
    public struct SpellHitStateDeps
    {
        public UnityEngine.InputSystem.PlayerInput slimeInput;
        public GameObject spellPrefab;
        public GameObject spellBeforeSpawnPrefab;
        public Transform pointToSpawnSpell;
        public Animator bossAnimator;
        public Camera mainCamera;
    }
    
    [System.Serializable]
    public struct SpellHitStateSet
    {
        public float waitTimeBeforeSpawn;
        public float spellSpeed;
        public float cameraShakeIntensity;
        public float cameraShakeDuration;
        public int cameraShakeVibrato;
        public float cameraShakeRandomness;
    }
    
    public class SpellHitLogic : ITutorialStateLogic
    {
        private static readonly int AttackMode = Animator.StringToHash("Attack mode");
        private static readonly int Idle = Animator.StringToHash("idle");
        private SpellHitStateDeps _spellHitStateDeps;
        private SpellHitStateSet _spellHitStateSet;
        private bool _slimeGetHit = false;
        private readonly OneSpellShotLogic _spellLogic;
        private readonly SpellSettings _spellSettings;
        
        public SpellHitLogic(SpellHitStateDeps spellHitStateDeps,
            SpellHitStateSet spellHitStateSet)
        {
            _spellHitStateDeps = spellHitStateDeps;
            _spellHitStateSet = spellHitStateSet;
            _SLIME.Slime.SlimeEvents.SlimeGetHit += OnSlimeGetHit;
            
            _spellSettings = new SpellSettings
            {
                attackAccuracyRange = Vector2.one,
                attackSpeedRange = new Vector2(spellHitStateSet.spellSpeed, spellHitStateSet.spellSpeed),
                targetMiddleProbability = 0f
            };
            
            _spellLogic = new OneSpellShotLogic(
                _spellHitStateDeps.spellPrefab,
                _spellHitStateDeps.spellBeforeSpawnPrefab,
                _spellHitStateDeps.pointToSpawnSpell,
                _spellHitStateDeps.pointToSpawnSpell);
        }
        
        public void OnDisable()
        {
            _SLIME.Slime.SlimeEvents.SlimeGetHit -= OnSlimeGetHit;
            _spellLogic.Reset();
        }
        
        public IEnumerator Start()
        {
            // DisableSlimeInput();
            
            yield return new WaitForSeconds(_spellHitStateSet.waitTimeBeforeSpawn);
            
            _spellLogic.Attack(_spellSettings);
            
            while (!_slimeGetHit)
            {
                // Update active attack logic if there is one (like BossSpawnAttackBehaviour line 93)
                if (_spellLogic.IsActive)
                {
                    _spellLogic.UpdateAttack();
                }
                else if (!_slimeGetHit)
                {
                    _spellHitStateDeps.bossAnimator.SetTrigger(AttackMode);
                    // Spell was spawned but slime wasn't hit, spawn another one
                    _spellLogic.Attack(_spellSettings);
                }
                
                yield return null;
            }

            _spellLogic.Reset();
            _spellHitStateDeps.bossAnimator.SetTrigger(Idle);
            ShakeCamera();
            
            // EnableSlimeInput();
        }
        
        private void DisableSlimeInput()
        {
            _spellHitStateDeps.slimeInput.enabled = false;
        }
        
        private void EnableSlimeInput()
        {
            _spellHitStateDeps.slimeInput.enabled = true;
        }
        
        private void OnSlimeGetHit()
        {
            _slimeGetHit = true;
        }
        
        private void ShakeCamera()
        {
            _spellHitStateDeps.mainCamera.transform.DOShakePosition(
                _spellHitStateSet.cameraShakeDuration,
                _spellHitStateSet.cameraShakeIntensity,
                _spellHitStateSet.cameraShakeVibrato,
                _spellHitStateSet.cameraShakeRandomness,
                false,
                true);
        }
    }
}
