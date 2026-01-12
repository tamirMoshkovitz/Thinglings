using System;
using System.Collections;
using _SLIME.BaseScripts;
using DG.Tweening;
using UnityEngine;

namespace _SLIME.Tutorial
{
    [System.Serializable]
    public enum TutorialState
    {
        RockState,
        RiseToBoss,
        SlimeConnects,
        SlimeTear,
        SpellHit,
        LearnSlimeToConnect,
        BossThrowsSpell,
        CaveShake,
        SceneMoveToFinalBattle
    }
    public class TutorialStateManager: ProjectMonoBehavior
    {
        
        [SerializeField] private TutorialState currentState;
        [SerializeField] private TutorialScriptable tutorialScriptable;
        
        public TutorialState CurrentState { get; private set; }
        
        private void Start()
        {
            CurrentState = currentState;
            StartCoroutine(GetCoroutineForState(currentState));
        }

        private void OnDisable()
        {
            if(_rockStateLogic != null) _rockStateLogic.OnDisable();
            if(_slimeTearsLogic != null) _slimeTearsLogic.OnDisable();  
        }

        private IEnumerator GetCoroutineForState(TutorialState state)
        {
            return state switch
            {
                TutorialState.RockState => RockStateCoroutine(),
                TutorialState.RiseToBoss => RiseToBossCoroutine(),
                TutorialState.SlimeConnects => SlimeConnectsCoroutine(),
                TutorialState.SlimeTear => SlimeTearCoroutine(),
                TutorialState.SpellHit => SpellHitCoroutine(),
                TutorialState.LearnSlimeToConnect => LearnSlimeToConnectCoroutine(),
                TutorialState.BossThrowsSpell => BossThrowsSpellCoroutine(),
                TutorialState.CaveShake => CaveShakeCoroutine(),
                TutorialState.SceneMoveToFinalBattle => SceneMoveToFinalBattleCoroutine(),
                _ => null
            };
        }
        
        #region RockState
        
        [SerializeField] private RockStateDeps rockStateDeps;
        private RockStateLogic _rockStateLogic;
        private IEnumerator RockStateCoroutine()
        {
            CurrentState = TutorialState.RockState;
            _rockStateLogic = new RockStateLogic(rockStateDeps,
                tutorialScriptable.RockStateSet);
            yield return _rockStateLogic.Start();
            StartCoroutine(RiseToBossCoroutine());
        }
        #endregion
        
        #region RiseToBoss
        
        [SerializeField] private RiseToBossStateDeps riseToBossStateDeps;
        

        private IEnumerator RiseToBossCoroutine()
        {
            CurrentState = TutorialState.RiseToBoss;
            yield return new RiseToBossLogic(riseToBossStateDeps,
                tutorialScriptable.RiseToBossStateSet).Start();
            StartCoroutine(SlimeConnectsCoroutine());
        }
        #endregion
        
        #region SlimeConnects
        [SerializeField] private SlimeConnectsStateDeps slimeConnectsStateDeps;
        
        private IEnumerator SlimeConnectsCoroutine()
        {
            CurrentState = TutorialState.SlimeConnects;
            yield return new SlimeConnectsLogic(slimeConnectsStateDeps,
                tutorialScriptable.SlimeConnectsStateSet).Start();
            StartCoroutine(SlimeTearCoroutine());
        }
        #endregion
        
        #region SlimeTear
        [SerializeField] private SlimeTearsStateDeps slimeTearsStateDeps;
        private SlimeTearsLogic _slimeTearsLogic;
        private IEnumerator SlimeTearCoroutine()
        {
            CurrentState = TutorialState.SlimeTear;

            _slimeTearsLogic = new SlimeTearsLogic(slimeTearsStateDeps,
                tutorialScriptable.SlimeTearsStateSet);
            yield return _slimeTearsLogic.Start();
            StartCoroutine(SpellHitCoroutine());
        }
        #endregion
        
        #region SpellHit
        private IEnumerator SpellHitCoroutine()
        {
            CurrentState = TutorialState.SpellHit;
            yield return null;
            StartCoroutine(LearnSlimeToConnectCoroutine());
        }
        #endregion
        
        #region LearnSlimeToConnect
        private IEnumerator LearnSlimeToConnectCoroutine()
        {
            CurrentState = TutorialState.LearnSlimeToConnect;
            yield return null;
            StartCoroutine(BossThrowsSpellCoroutine());
        }
        #endregion
        
        #region BossThrowsSpell
        private IEnumerator BossThrowsSpellCoroutine()
        {
            CurrentState = TutorialState.BossThrowsSpell;
            yield return null;
            StartCoroutine(CaveShakeCoroutine());
        }
        #endregion
        
        #region CaveShake
        private IEnumerator CaveShakeCoroutine()
        {
            CurrentState = TutorialState.CaveShake;
            yield return null;
            StartCoroutine(SceneMoveToFinalBattleCoroutine());
        }
        #endregion
        
        #region SceneMoveToFinalBattle
        private IEnumerator SceneMoveToFinalBattleCoroutine()
        {
            CurrentState = TutorialState.SceneMoveToFinalBattle;
            yield return null;
        }
        #endregion
    }

    
}