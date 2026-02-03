using System;
using System.Collections;
using _SLIME.Projectiles;
using DG.Tweening;
using UnityEngine;

namespace _SLIME.Tutorial
{
    [System.Serializable]
    public struct SpellHitStateDeps
    {
        public UnityEngine.InputSystem.PlayerInput slimeInput;
        public GameObject spellPrefab;
        public Transform pointToSpawnSpell;
        public Transform slime1;
        public Transform slime2;
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
        private SpellHitStateDeps _spellHitStateDeps;
        private SpellHitStateSet _spellHitStateSet;
        private bool _slimeGetHit = false;
        private GameObject _currentSpell;
        private Vector3 _currentSpawnPosition;
        
        public SpellHitLogic(SpellHitStateDeps spellHitStateDeps,
            SpellHitStateSet spellHitStateSet)
        {
            _spellHitStateDeps = spellHitStateDeps;
            _spellHitStateSet = spellHitStateSet;
            _SLIME.Slime.SlimeEvents.SlimeGetHit += OnSlimeGetHit;
        }
        
        public void OnDisable()
        {
            _SLIME.Slime.SlimeEvents.SlimeGetHit -= OnSlimeGetHit;
        }
        
        public IEnumerator Start()
        {
            // DisableSlimeInput();
            
            yield return new WaitForSeconds(_spellHitStateSet.waitTimeBeforeSpawn);
            
            SpawnAndLaunchSpell();
            
            while (!_slimeGetHit)
            {
                // Check if current spell was destroyed without hitting
                if (_currentSpell == null)
                {
                    // Spell was destroyed without hitting slime, spawn a new one
                    SpawnAndLaunchSpell();
                }
                yield return null;
            }
            
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
        
        private void SpawnAndLaunchSpell()
        {
            _currentSpawnPosition = _spellHitStateDeps.pointToSpawnSpell.position;
            _currentSpell = GameObject.Instantiate(_spellHitStateDeps.spellPrefab, 
                _currentSpawnPosition, Quaternion.identity);
            
            Vector3 slime1Pos = _spellHitStateDeps.slime1.position;
            Vector3 slime2Pos = _spellHitStateDeps.slime2.position;
            
            Vector3 spawnTarget = slime1Pos.x < slime2Pos.x ? slime1Pos : slime2Pos;
           

            Spell spell = _currentSpell.GetComponentInChildren<Spell>();
            spell.BossSetup(new SpellBossAttributes
            {
                spawnPosition = _currentSpawnPosition,
                targetPosition = spawnTarget,
                moveSpeed = _spellHitStateSet.spellSpeed,
            });

        }
        
        private void OnSlimeGetHit()
        {
            _slimeGetHit = true;
        }
        
        private IEnumerator WaitForSlimeGetHit()
        {
            while (!_slimeGetHit)
            {
                yield return null;
            }
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
