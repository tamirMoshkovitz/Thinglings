using System.Collections.Generic;
using _SLIME.Slime;
using NaughtyAttributes;
using UnityEngine;

namespace _SLIME.Boss
{
    public enum PossibleAttacks
    {
        OneShot, // Main logic of attacks. One shot implementation
        TwoShots, // ONE SHOT TOWARDS slime1 AND THE OTHER TO slime2.
        ThreeShot, // THREE SHOTS TOGETHER, TWO SHOTS + One Shot
                   // (to the middle between the slimes)
        FourShots, // Combination of ONE SHOT four times in consecutive intervals, with a wait between each shot
        BulletHell, // 8 Shots in consecutive intervals in cyclic order
    }
    
    [System.Serializable]
    public struct SpawnSettings
    {
        [MinMaxSlider(0f,5f)]
        public Vector2 delayBetweenAttacks;
        
        public AttackProbabilities bothSlimesAliveProbabilities;
        
        public AttackProbabilities notConnectedProbabilities;
        
        public AttackProbabilities oneSlimeAliveProbabilities;
    }
    
    public class BossSpawnAttackBehaviour : BossBaseBehaviour
    {
        private static readonly int AttackFinished = Animator.StringToHash("AttackFinished");

        private int _attackCounter;
        private float _timer;
        private float _currentDelay;
        
        private Dictionary<PossibleAttacks, ISpellAttackLogic> _attackLogics;
        private ISpellAttackLogic _currentActiveLogic;

        public override void Initialize(BossBrain brain)
        {
            base.Initialize(brain);
            
            var oneSpellShotLogic = new OneSpellShotLogic(
                BossBrain.bossConfigurations.SpawnAttack.projectilePrefab,
                Data
            );
            
            _attackLogics = new Dictionary<PossibleAttacks, ISpellAttackLogic>
            {
                { PossibleAttacks.OneShot, oneSpellShotLogic },
                { PossibleAttacks.TwoShots, new TwoShotsLogic(oneSpellShotLogic) },
                { PossibleAttacks.ThreeShot, new ThreeShotsLogic(oneSpellShotLogic, Data) },
                { PossibleAttacks.FourShots, new FourShotsLogic(oneSpellShotLogic, Data) },
                { PossibleAttacks.BulletHell, new BulletHellLogic(oneSpellShotLogic, Data) },
            };
        }

        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            base.OnStateEnter(animator, stateInfo, layerIndex);
            Data.BossFarState();
            TotalAttacksPreformed++;
            _attackCounter = 0;
            _timer = 0f;
            _currentDelay = 0f;
        }

        public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            // If there's an active attack logic, update it and wait
            if (_currentActiveLogic != null && _currentActiveLogic.IsActive)
            {
                _currentActiveLogic.UpdateAttack();
                return;
            }
            
            if (_attackCounter >= BossBrain.bossConfigurations.SpawnAttack.attacksToCast
            || Data.WaterStateActivated)
            {
                animator.SetTrigger(AttackFinished);
                return;
            }
            
            _timer += Time.deltaTime;
            
            if (_timer >= _currentDelay)
            {
                PossibleAttacks chosenAttack = ChooseAttackByProbability();
                ExecuteAttack(chosenAttack);
                _attackCounter++;
                _timer = 0f;
            
                Vector2 delayRange = BossBrain.bossConfigurations.SpawnAttack.spawnSettings.delayBetweenAttacks;
                _currentDelay = Random.Range(delayRange.x, delayRange.y);
            }
        }
        
        private PossibleAttacks ChooseAttackByProbability()
        {
            bool bothAlive = !SlimeData.instance.SideADead && !SlimeData.instance.SideBDead;
            
            AttackProbabilities probabilities;
            
            if (!bothAlive)
            {
                probabilities = BossBrain.bossConfigurations.SpawnAttack.spawnSettings.oneSlimeAliveProbabilities;
            }
            else if (!Data.slimesConnected)
            {
                probabilities = BossBrain.bossConfigurations.SpawnAttack.spawnSettings.notConnectedProbabilities;
            }
            else
            {
                probabilities = BossBrain.bossConfigurations.SpawnAttack.spawnSettings.bothSlimesAliveProbabilities;
            }
                
            return probabilities.GetRandomAttack();
        }
        
        private void ExecuteAttack(PossibleAttacks attack)
        {
            if (_attackLogics.TryGetValue(attack, out var logic))
            {
                _currentActiveLogic = logic;
                logic.Attack(BossBrain.bossConfigurations.SpawnAttack.spellSettings);
            }
        }

        public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            base.OnStateExit(animator, stateInfo, layerIndex);
            
            foreach (var logic in _attackLogics.Values)
            {
                logic.Reset();
            }
            
            _currentActiveLogic = null;
        }
    }
}
