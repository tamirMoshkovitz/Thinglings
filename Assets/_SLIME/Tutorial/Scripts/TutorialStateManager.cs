using System;
using System.Collections;
using System.Collections.Generic;
using _SLIME.BaseScripts;
using _SLIME.Core.Audio.FMOD.Scripts;
using _SLIME.Slime;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Serialization;
using EventReference = FMODUnity.EventReference;

namespace _SLIME.Tutorial
{
    [System.Serializable]
    public enum TutorialState
    {
        LogoState,
        RockState,
        RiseToBoss,
        SlimeConnects,
        SlimeTear,
        SpellHit,
        LearnSlimeToConnect,
        LearnSlimeAboutSpells,
        BossThrowsSpell,
        CaveShake,
        SceneMoveToFinalBattle
    }
    public class TutorialStateManager: ProjectMonoBehavior
    {
        
        [SerializeField] private TutorialState currentState;
        [SerializeField] private TutorialScriptable tutorialScriptable;
        
        [Header("End Animation SFX")]
        [SerializeField] private EventReference bossFlySFX;
        [SerializeField] private float endAnimationSFXDelay = 3f;
        
        public TutorialState CurrentState { get; private set; }
        
        private List<ITutorialStateLogic> _allLogics = new List<ITutorialStateLogic>();
        
        private void Start()
        {
            CurrentState = currentState;
            StartCoroutine(GetCoroutineForState(currentState));
        }

        private void OnDisable()
        {
            foreach (var logic in _allLogics)
            {
                logic?.OnDisable();
            }
            _allLogics.Clear();
        }

        private IEnumerator GetCoroutineForState(TutorialState state)
        {
            return state switch
            {
                TutorialState.LogoState => LogoStateCoroutine(),
                TutorialState.RockState => RockStateCoroutine(),
                TutorialState.RiseToBoss => RiseToBossCoroutine(),
                TutorialState.SlimeConnects => SlimeConnectsCoroutine(),
                TutorialState.SlimeTear => SlimeTearCoroutine(),
                TutorialState.SpellHit => SpellHitCoroutine(),
                TutorialState.LearnSlimeToConnect => LearnSlimeToConnectCoroutine(),
                TutorialState.LearnSlimeAboutSpells => LearnSlimeAboutSpellsCoroutine(),
                TutorialState.BossThrowsSpell => BossThrowsSpellCoroutine(),
                TutorialState.CaveShake => CaveShakeCoroutine(),
                TutorialState.SceneMoveToFinalBattle => SceneMoveToFinalBattleCoroutine(),
                _ => null
            };
        }
        
        
        #region LogoState
        
        [SerializeField] private LogoStateDeps logoStateDeps;
        private IEnumerator LogoStateCoroutine()
        {
            CurrentState = TutorialState.LogoState;
            var logic = new LogoStateLogic(logoStateDeps, tutorialScriptable.LogoStateSet);
            _allLogics.Add(logic);
            yield return logic.Start();
            StartCoroutine(RockStateCoroutine());
        }
        
        #endregion
        
        #region RockState
        
        [SerializeField] private RockStateDeps rockStateDeps;
        private IEnumerator RockStateCoroutine()
        {
            CurrentState = TutorialState.RockState;
            var logic = new RockStateLogic(rockStateDeps, tutorialScriptable.RockStateSet);
            _allLogics.Add(logic);
            yield return logic.Start();
            StartCoroutine(RiseToBossCoroutine());
        }
        #endregion
        
        #region RiseToBoss
        
        [SerializeField] private RiseToBossStateDeps riseToBossStateDeps;
        
        private IEnumerator RiseToBossCoroutine()
        {
            
            CurrentState = TutorialState.RiseToBoss;
            var logic = new RiseToBossLogic(riseToBossStateDeps, tutorialScriptable.RiseToBossStateSet);
            _allLogics.Add(logic);
            yield return logic.Start();
            StartCoroutine(SlimeTearCoroutine());
        }
        #endregion
        // TODO: FOR NOW THIS STATE IS NOT ACTIVE, AND MAYBE FOREVER
        #region SlimeConnects
        [SerializeField] private SlimeConnectsStateDeps slimeConnectsStateDeps;
        
        private IEnumerator SlimeConnectsCoroutine()
        {
            CurrentState = TutorialState.SlimeConnects;
            var logic = new SlimeConnectsLogic(slimeConnectsStateDeps, tutorialScriptable.SlimeConnectsStateSet);
            _allLogics.Add(logic);
            yield return logic.Start();
            StartCoroutine(SlimeTearCoroutine());
        }
        #endregion
        
        #region SlimeTear
        [SerializeField] private SlimeTearsStateDeps slimeTearsStateDeps;
        
        private IEnumerator SlimeTearCoroutine()
        {
            CurrentState = TutorialState.SlimeTear;
            var logic = new SlimeTearsLogic(slimeTearsStateDeps, tutorialScriptable.SlimeTearsStateSet);
            _allLogics.Add(logic);
            yield return logic.Start();
            StartCoroutine(SpellHitCoroutine());
        }
        #endregion
        
        #region SpellHit
        [SerializeField] private SpellHitStateDeps spellHitStateDeps;
        
        private IEnumerator SpellHitCoroutine()
        {
            CurrentState = TutorialState.SpellHit;
            var logic = new SpellHitLogic(spellHitStateDeps, tutorialScriptable.SpellHitStateSet);
            _allLogics.Add(logic);
            yield return logic.Start();
            StartCoroutine(LearnSlimeToConnectCoroutine());
        }
        #endregion
        
        #region LearnSlimeToConnect
        [SerializeField] private LearnSlimeToConnectStateDeps learnSlimeToConnectStateDeps;
        
        private IEnumerator LearnSlimeToConnectCoroutine()
        {
            CurrentState = TutorialState.LearnSlimeToConnect;
            var logic = new LearnSlimeToConnectLogic(learnSlimeToConnectStateDeps, tutorialScriptable.LearnSlimeToConnectStateSet);
            _allLogics.Add(logic);
            yield return logic.Start();
            StartCoroutine(BossThrowsSpellCoroutine());
        }
        #endregion

        
        // TODO: FOR NOW THIS STATE IS NOT ACTIVE, AND MAYBE FOREVER
        #region LearnSlimeAboutSpells
        [SerializeField] private LearnSlimeAboutSpellsStateDeps learnSlimeAboutSpellsStateDeps;
        
        private IEnumerator LearnSlimeAboutSpellsCoroutine()
        {
            CurrentState = TutorialState.LearnSlimeAboutSpells;
            var logic = new LearnSlimeAboutSpellsLogic(learnSlimeAboutSpellsStateDeps, tutorialScriptable.LearnSlimeAboutSpellsStateSet);
            _allLogics.Add(logic);
            yield return logic.Start();
            StartCoroutine(BossThrowsSpellCoroutine());
        }
        #endregion
        
        #region BossThrowsSpell
        [SerializeField] private BossThrowsSpellStateDeps bossThrowsSpellStateDeps;
        
        private IEnumerator BossThrowsSpellCoroutine()
        {
            CurrentState = TutorialState.BossThrowsSpell;
            var logic = new BossThrowsSpellLogic(bossThrowsSpellStateDeps, tutorialScriptable.BossThrowsSpellStateSet);
            _allLogics.Add(logic);
            yield return logic.Start();
            StartCoroutine(CaveShakeCoroutine());
        }
        #endregion
        
        #region CaveShake
        [SerializeField] private CaveShakeStateDeps caveShakeStateDeps;
        
        private IEnumerator CaveShakeCoroutine()
        {
            CurrentState = TutorialState.CaveShake;
            var logic = new CaveShakeLogic(caveShakeStateDeps, tutorialScriptable.CaveShakeStateSet);
            _allLogics.Add(logic);
            yield return logic.Start();
            StartCoroutine(SceneMoveToFinalBattleCoroutine());
        }
        #endregion
        
        #region SceneMoveToFinalBattle
        [SerializeField] private SceneMoveToFinalBattleStateDeps sceneMoveToFinalBattleStateDeps;
        
        private IEnumerator SceneMoveToFinalBattleCoroutine()
        {
            StartCoroutine(PlayMoveToFinalBattleSFX(endAnimationSFXDelay));
            CurrentState = TutorialState.SceneMoveToFinalBattle;
            var logic = new SceneMoveToFinalBattleLogic(sceneMoveToFinalBattleStateDeps, tutorialScriptable.SceneMoveToFinalBattleStateSet);
            _allLogics.Add(logic);
            yield return logic.Start();
        }
        
        private IEnumerator PlayMoveToFinalBattleSFX(float delay)
        {
            yield return new WaitForSeconds(delay);
            SFXPlayer.Play(bossFlySFX);
        }
        #endregion
    }
    
}
