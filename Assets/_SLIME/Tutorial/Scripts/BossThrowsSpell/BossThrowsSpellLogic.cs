using System;
using System.Collections;
using _SLIME.Projectiles;
using UnityEngine;

namespace _SLIME.Tutorial
{
    [System.Serializable]
    public struct BossThrowsSpellStateDeps
    {
        public Transform slime1;
        public Transform slime2;
        public Transform leftSpellSpawnPoint;
        public Transform rightSpellSpawnPoint;
        public GameObject spellPrefab;
        public Animator animatorForSketch;
    }
    
    [System.Serializable]
    public struct BossThrowsSpellStateSet
    {
        public float timeBetweenSpells;
        public float spellSpeed;
    }
    
    public class BossThrowsSpellLogic : ITutorialStateLogic
    {
        private static readonly int SpellReturn = Animator.StringToHash("spell return");
        private BossThrowsSpellStateDeps _deps;
        private BossThrowsSpellStateSet _set;
        private bool _bossHit;
        
        public BossThrowsSpellLogic(BossThrowsSpellStateDeps bossThrowsSpellStateDeps,
            BossThrowsSpellStateSet bossThrowsSpellStateSet)
        {
            _deps = bossThrowsSpellStateDeps;
            _set = bossThrowsSpellStateSet;
            _bossHit = false;
            TutorialBoss.BossHit += OnBossHit;
        }
        
        public void OnDisable()
        {
            TutorialBoss.BossHit -= OnBossHit;
        }
        
        public IEnumerator Start()
        {
            _deps.animatorForSketch.SetTrigger(SpellReturn);
            yield return ThrowSpellsUntilBossHit();
        }
        
        private IEnumerator ThrowSpellsUntilBossHit()
        {
            bool useLeftSpawn = true;
            float timeSinceLastSpell = 0f;
            
            while (!_bossHit)
            {
                timeSinceLastSpell += Time.deltaTime;
                
                if (timeSinceLastSpell >= _set.timeBetweenSpells)
                {
                    Vector3 centerBetweenSlimes = (_deps.slime1.position + _deps.slime2.position) / 2f;
                    Transform spawnPoint = useLeftSpawn ? _deps.leftSpellSpawnPoint : _deps.rightSpellSpawnPoint;
                    useLeftSpawn = !useLeftSpawn;
                    
                    SpawnSpell(spawnPoint, centerBetweenSlimes);
                    timeSinceLastSpell = 0f;
                }
                
                yield return null;
            }
            _deps.animatorForSketch.gameObject.SetActive(false);
        }
        
        private void SpawnSpell(Transform spawnPoint, Vector3 targetPosition)
        {
            // Spawn spell
            GameObject spellGO = GameObject.Instantiate(_deps.spellPrefab, spawnPoint.position, Quaternion.identity);
            
            // Calculate direction to target
            Vector3 direction = (targetPosition - spawnPoint.position).normalized;
            
            Spell spell = spellGO.GetComponentInChildren<Spell>(); 
            spell.BossSetup(new SpellBossAttributes
            {
                direction = direction,
                moveSpeed = _set.spellSpeed,
            });
        }
        
        private void OnBossHit()
        {
            _bossHit = true;
        }
    }
}
