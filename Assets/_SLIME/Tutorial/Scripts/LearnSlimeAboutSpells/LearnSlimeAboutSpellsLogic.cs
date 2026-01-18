using System;
using System.Collections;
using _SLIME.Tutorial;
using DG.Tweening;
using UnityEngine;

namespace _SLIME.Tutorial
{
    [System.Serializable]
    public struct LearnSlimeAboutSpellsStateDeps
    {
        public Transform slime1;
        public Transform slime2;
        public Transform bossHitPoint;
        public Transform leftSpellSpawnPoint;
        public Transform rightSpellSpawnPoint;
        public GameObject spellPrefab;
        public UnityEngine.InputSystem.PlayerInput slimeInput;
        
    }
    
    [System.Serializable]
    public struct LearnSlimeAboutSpellsStateSet
    {
        public float separationDistance;
        public float separationTime;
        public AnimationCurve separationCurve;
        public float spellSpeed;
    }
    
    public class LearnSlimeAboutSpellsLogic : ITutorialStateLogic
    {
        private LearnSlimeAboutSpellsStateDeps _deps;
        private LearnSlimeAboutSpellsStateSet _set;
        private bool _bossHit;
        
        public LearnSlimeAboutSpellsLogic(LearnSlimeAboutSpellsStateDeps learnSlimeAboutSpellsStateDeps,
            LearnSlimeAboutSpellsStateSet learnSlimeAboutSpellsStateSet)
        {
            _deps = learnSlimeAboutSpellsStateDeps;
            _set = learnSlimeAboutSpellsStateSet;
        }
        
        public void OnDisable()
        {
            TutorialBoss.BossHit -= OnBossHit;
        }
        
        public IEnumerator Start()
        {
            DisableSlimeInput();
            
            // Determine spawn point based on current slime positions
            Vector3 currentCenter = (_deps.slime1.position + _deps.slime2.position) / 2f;
            Transform spawnPoint = DetermineSpawnPoint(currentCenter);
            
            yield return SeparateSlimes(spawnPoint);
            
            SpawnAndLaunchSpell(spawnPoint);
            
            _bossHit = false;
            TutorialBoss.BossHit += OnBossHit;
            yield return WaitForBossHit();
            
            EnableSlimeInput();
        }
        
        private void DisableSlimeInput()
        {
            _deps.slimeInput.DeactivateInput();
        }
        
        private void EnableSlimeInput()
        {
            _deps.slimeInput.ActivateInput();
        }
        
        private IEnumerator SeparateSlimes(Transform spawnPoint)
        {
            Vector3 bossPos = _deps.bossHitPoint.position;
            Vector3 spawnPos = spawnPoint.position;
            
            // Calculate the line from spawn point to boss
            Vector3 spellDirection = (bossPos - spawnPos).normalized;
            
            // Calculate perpendicular direction for slimes separation (horizontal perpendicular)
            Vector3 perpendicular = new Vector3(-spellDirection.y, spellDirection.x, 0f).normalized;
            
            // Find a point on the line from spawn to boss where we want the center to be
            // Let's put it at a reasonable distance from spawn point
            float distanceAlongLine = Vector3.Distance(spawnPos, bossPos) * 0.5f; // Midway
            Vector3 centerTarget = spawnPos + spellDirection * distanceAlongLine;
            
            // Calculate target positions for slimes (perpendicular to spell path)
            Vector3 slime1Target = centerTarget - perpendicular * (_set.separationDistance / 2f);
            Vector3 slime2Target = centerTarget + perpendicular * (_set.separationDistance / 2f);
            
            // Move slimes to target positions
            Sequence sequence = DOTween.Sequence();
            sequence.Append(_deps.slime1.DOMove(slime1Target, _set.separationTime).SetEase(_set.separationCurve));
            sequence.Join(_deps.slime2.DOMove(slime2Target, _set.separationTime).SetEase(_set.separationCurve));
            
            yield return sequence.WaitForCompletion();
        }
        
        private void SpawnAndLaunchSpell(Transform spawnPoint)
        {
            // Calculate center between slimes
            Vector3 centerBetweenSlimes = (_deps.slime1.position + _deps.slime2.position) / 2f;
            
            // Spawn spell
            GameObject spell = GameObject.Instantiate(_deps.spellPrefab, spawnPoint.position, Quaternion.identity);
            
            // Calculate direction to center
            Vector3 direction = (centerBetweenSlimes - spawnPoint.position).normalized;
            
            // Set velocity
            Rigidbody2D rb = spell.GetComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            rb.linearVelocity = direction * _set.spellSpeed;
        }
        
        private Transform DetermineSpawnPoint(Vector3 centerPos)
        {
            float leftX = _deps.leftSpellSpawnPoint.position.x;
            float rightX = _deps.rightSpellSpawnPoint.position.x;
            float centerX = centerPos.x;
            
            // If center is to the right of rightSpawnPoint, shoot from left
            if (centerX > rightX)
            {
                return _deps.leftSpellSpawnPoint;
            }
            // If center is to the left of leftSpawnPoint, shoot from right
            else if (centerX < leftX)
            {
                return _deps.rightSpellSpawnPoint;
            }
            // If center is between spawn points, shoot from the farther one
            else
            {
                float distToLeft = Mathf.Abs(centerX - leftX);
                float distToRight = Mathf.Abs(centerX - rightX);
                
                return distToLeft > distToRight ? _deps.leftSpellSpawnPoint : _deps.rightSpellSpawnPoint;
            }
        }
        
        private void OnBossHit()
        {
            _bossHit = true;
        }
        
        private IEnumerator WaitForBossHit()
        {
            while (!_bossHit)
            {
                yield return null;
            }
        }
    }
}
