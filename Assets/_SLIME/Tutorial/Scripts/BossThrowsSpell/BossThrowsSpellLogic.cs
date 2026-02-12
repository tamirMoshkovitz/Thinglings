using System;
using System.Collections;
using _SLIME.Projectiles;
using _SLIME.Boss;
using UnityEngine;
using UnityEngine.UI;

namespace _SLIME.Tutorial
{
    [System.Serializable]
    public struct BossThrowsSpellStateDeps
    {
        public Transform slime1;
        public Transform slime2;
        public Transform spellSpawnPoint;
        public GameObject spellPrefab;
        public GameObject spellBeforeSpawnPrefab;
        public Animator animatorForSketch;
        public Animator bossAnimator;
        public Image bossHealthBar;
        public GameObject bossHealthBarCanvas;
    }
    
    [System.Serializable]
    public struct BossThrowsSpellStateSet
    {
        public float timeBetweenSpells;
        public float spellSpeed;
        
        public float[] bossHealthBarThresholds;
    }
    
    public class BossThrowsSpellLogic : ITutorialStateLogic
    {
        private static readonly int AttackMode = Animator.StringToHash("Attack mode");
        private const int HitsToComplete = 3;
        private static readonly int SpellReturn = Animator.StringToHash("spell return");
        private static readonly int Idle = Animator.StringToHash("idle");
        private BossThrowsSpellStateDeps _deps;
        private BossThrowsSpellStateSet _set;
        private int _hitCount;
        private readonly OneSpellShotLogic _spellLogic;
        private readonly SpellSettings _spellSettings;
        
        public BossThrowsSpellLogic(BossThrowsSpellStateDeps bossThrowsSpellStateDeps,
            BossThrowsSpellStateSet bossThrowsSpellStateSet)
        {
            _deps = bossThrowsSpellStateDeps;
            _set = bossThrowsSpellStateSet;
            _hitCount = 0;
            TutorialBoss.BossHit += OnBossHit;

            _spellSettings = new SpellSettings
            {
                attackAccuracyRange = Vector2.one,
                attackSpeedRange = new Vector2(_set.spellSpeed, _set.spellSpeed),
                targetMiddleProbability = 1f
            };
            
            _spellLogic = new OneSpellShotLogic(
                _deps.spellPrefab,
                _deps.spellBeforeSpawnPrefab,
                _deps.spellSpawnPoint,
                _deps.spellSpawnPoint);
        }
        
        public void OnDisable()
        {
            TutorialBoss.BossHit -= OnBossHit;
        }
        
        public IEnumerator Start()
        {
            if (_deps.bossHealthBarCanvas != null)
                _deps.bossHealthBarCanvas.SetActive(true);
            UpdateBossHealthBar(0);
            
            _deps.animatorForSketch.SetTrigger(SpellReturn);
            yield return ThrowSpellsUntilBossHit();
            _spellLogic.Reset();
        }
        
        private IEnumerator ThrowSpellsUntilBossHit()
        {
            float timeSinceLastSpell = 0f;
            
            while (_hitCount < HitsToComplete)
            {
                // Update active attack logic if there is one (like BossSpawnAttackBehaviour line 93)
                if (_spellLogic.IsActive)
                {
                    _spellLogic.UpdateAttack();
                    yield return null;
                    continue;
                }
                
                timeSinceLastSpell += Time.deltaTime;
                
                if (timeSinceLastSpell >= _set.timeBetweenSpells)
                {
                    _deps.bossAnimator.SetTrigger(AttackMode);
                    _spellLogic.Attack(_spellSettings);
                    timeSinceLastSpell = 0f;
                }
                
                yield return null;
            }

            _spellLogic.Reset();
            _deps.bossAnimator.SetTrigger(Idle);
            _deps.animatorForSketch.gameObject.SetActive(false);
            if (_deps.bossHealthBarCanvas != null)
                _deps.bossHealthBarCanvas.SetActive(false);
            
        }
        
        private void UpdateBossHealthBar(int hitIndex)
        {
            if (_deps.bossHealthBar == null || _set.bossHealthBarThresholds == null) return;
            if (hitIndex < 0 || hitIndex >= _set.bossHealthBarThresholds.Length) return;
            _deps.bossHealthBar.fillAmount = _set.bossHealthBarThresholds[hitIndex];
        }
        
        private void OnBossHit()
        {
            if (_hitCount >= HitsToComplete) return;
            _hitCount++;
            UpdateBossHealthBar(_hitCount);
        }
    }
}
