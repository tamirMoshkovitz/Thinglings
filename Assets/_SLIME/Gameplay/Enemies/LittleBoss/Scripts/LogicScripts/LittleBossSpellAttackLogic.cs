using System;
using System.Collections.Generic;
using _SLIME.Boss;
using _SLIME.Slime;
using UnityEngine;

namespace _SLIME.LittleBoss
{
    [Serializable]
    public struct LittleBossSpawnSettings
    {
        public AttackProbabilities bothSlimesAliveProbabilities;
        public AttackProbabilities notConnectedProbabilities;
        public AttackProbabilities oneSlimeAliveProbabilities;
    }

    [Serializable]
    public struct LittleBossSpellAttackSettings
    {
        public SpellSettings spellSettings;
        public LittleBossSpawnSettings spawnSettings;
        public SpellSpecialAttacksSettings specialAttacksSettings;
    }

    [Serializable]
    public struct LittleBossSpellAttackRef
    {
        public GameObject spellPrefab;
        public GameObject spellBeforeSpawnPrefab;
        public Transform spawnPoint;
        public Transform parent;
        public Animator animator;
    }

    public class LittleBossSpellAttackLogic : LittleBossBaseLogic
    {
        private static readonly int FInishAttack = Animator.StringToHash("FInishAttack");
        public LittleBossSpellAttackSettings Set;
        private readonly LittleBossSpellAttackRef _ref;
        private Dictionary<PossibleAttacks, ISpellAttackLogic> _attackLogics;
        private ISpellAttackLogic _currentActiveLogic;

        public LittleBossSpellAttackLogic(LittleBossSpellAttackSettings set,
            LittleBossSpellAttackRef reference)
        {
            Set = set;
            _ref = reference;

            var oneSpellShotLogic = new OneSpellShotLogic(_ref.spellPrefab, _ref.spellBeforeSpawnPrefab,
                _ref.spawnPoint, _ref.parent);

            var specials = Set.specialAttacksSettings;
            _attackLogics = new Dictionary<PossibleAttacks, ISpellAttackLogic>
            {
                { PossibleAttacks.OneShot, oneSpellShotLogic },
                { PossibleAttacks.TwoShots, new TwoShotsLogic(oneSpellShotLogic) },
                { PossibleAttacks.ThreeShot, new ThreeShotsLogic(oneSpellShotLogic, _ref.spawnPoint, specials) },
                { PossibleAttacks.FourShots, new FourShotsLogic(oneSpellShotLogic, specials) },
                { PossibleAttacks.BulletHell, new BulletHellLogic(oneSpellShotLogic, _ref.spawnPoint, specials) },
            };
        }

        public void StartLogic()
        {
            if (_ref.spellPrefab == null || _ref.spawnPoint == null) return;

            var chosenAttack = ChooseAttackByProbability();
            if (_attackLogics.TryGetValue(chosenAttack, out var logic))
            {
                _currentActiveLogic = logic;
                logic.Attack(Set.spellSettings);
            }
        }

        public void EndLogic()
        {
            foreach (var logic in _attackLogics.Values)
                logic.Reset();
            _currentActiveLogic = null;
        }

        public void UpdateLogic()
        {
            if (!_currentActiveLogic.IsActive) return;

            _currentActiveLogic.UpdateAttack();

            if (!_currentActiveLogic.IsActive && _ref.animator != null)
            {
                _ref.animator.SetTrigger(FInishAttack);
                _currentActiveLogic = null;
            }
        }

        private PossibleAttacks ChooseAttackByProbability()
        {
            if (SlimeData.instance == null) return PossibleAttacks.OneShot;

            bool bothAlive = !SlimeData.instance.SideADead && !SlimeData.instance.SideBDead;
            AttackProbabilities probabilities;

            if (!bothAlive)
                probabilities = Set.spawnSettings.oneSlimeAliveProbabilities;
            else if (!SlimeData.instance.Connected)
                probabilities = Set.spawnSettings.notConnectedProbabilities;
            else
                probabilities = Set.spawnSettings.bothSlimesAliveProbabilities;

            if (probabilities.Total <= 0f) return PossibleAttacks.OneShot;
            return probabilities.GetRandomAttack();
        }
    }
}