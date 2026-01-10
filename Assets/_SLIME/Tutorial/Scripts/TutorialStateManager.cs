using System;
using System.Collections;
using _SLIME.BaseScripts;
using UnityEngine;

namespace _SLIME.Tutorial
{
    [System.Serializable]
    public enum TutorialState
    {
        RockState,
        RockExplosion,
        RiseToBoss,
        SlimeConnects,
        SlimeTear,
        BossGetsAngry,
        ZoomOut,
        SpellHit,
        LearnSlimeToConnect,
        BossThrowsSpell,
        CaveShake,
        SceneMoveToFinalBattle
    }
    public class TutorialStateManager: ProjectMonoBehavior
    {
        
        [SerializeField] private TutorialState currentState;
        
        private void Start()
        {
            StartCoroutine(GetCoroutineForState(currentState));
        }
        
        private IEnumerator GetCoroutineForState(TutorialState state)
        {
            return state switch
            {
                TutorialState.RockState => RockStateCoroutine(),
                TutorialState.RockExplosion => RockExplosionCoroutine(),
                TutorialState.RiseToBoss => RiseToBossCoroutine(),
                TutorialState.SlimeConnects => SlimeConnectsCoroutine(),
                TutorialState.SlimeTear => SlimeTearCoroutine(),
                TutorialState.BossGetsAngry => BossGetsAngryCoroutine(),
                TutorialState.ZoomOut => ZoomOutCoroutine(),
                TutorialState.SpellHit => SpellHitCoroutine(),
                TutorialState.LearnSlimeToConnect => LearnSlimeToConnectCoroutine(),
                TutorialState.BossThrowsSpell => BossThrowsSpellCoroutine(),
                TutorialState.CaveShake => CaveShakeCoroutine(),
                TutorialState.SceneMoveToFinalBattle => SceneMoveToFinalBattleCoroutine(),
                _ => null
            };
        }
        
        #region RockState
        
        [Serializable]
        private struct RockStateDeps
        {
            public GameObject rock;
        }
        [SerializeField] private RockStateDeps rockStateDeps;
        private IEnumerator RockStateCoroutine()
        {
            yield return null;
            StartCoroutine(RockExplosionCoroutine());
        }
        #endregion
        
        #region RockExplosion
        private IEnumerator RockExplosionCoroutine()
        {
            yield return null;
            StartCoroutine(RiseToBossCoroutine());
        }
        #endregion
        
        #region RiseToBoss
        private IEnumerator RiseToBossCoroutine()
        {
            yield return null;
            StartCoroutine(SlimeConnectsCoroutine());
        }
        #endregion
        
        #region SlimeConnects
        private IEnumerator SlimeConnectsCoroutine()
        {
            yield return null;
            StartCoroutine(SlimeTearCoroutine());
        }
        #endregion
        
        #region SlimeTear
        private IEnumerator SlimeTearCoroutine()
        {
            yield return null;
            StartCoroutine(BossGetsAngryCoroutine());
        }
        #endregion
        
        #region BossGetsAngry
        private IEnumerator BossGetsAngryCoroutine()
        {
            yield return null;
            StartCoroutine(ZoomOutCoroutine());
        }
        #endregion
        
        #region ZoomOut
        private IEnumerator ZoomOutCoroutine()
        {
            yield return null;
            StartCoroutine(SpellHitCoroutine());
        }
        #endregion
        
        #region SpellHit
        private IEnumerator SpellHitCoroutine()
        {
            yield return null;
            StartCoroutine(LearnSlimeToConnectCoroutine());
        }
        #endregion
        
        #region LearnSlimeToConnect
        private IEnumerator LearnSlimeToConnectCoroutine()
        {
            yield return null;
            StartCoroutine(BossThrowsSpellCoroutine());
        }
        #endregion
        
        #region BossThrowsSpell
        private IEnumerator BossThrowsSpellCoroutine()
        {
            yield return null;
            StartCoroutine(CaveShakeCoroutine());
        }
        #endregion
        
        #region CaveShake
        private IEnumerator CaveShakeCoroutine()
        {
            yield return null;
            StartCoroutine(SceneMoveToFinalBattleCoroutine());
        }
        #endregion
        
        #region SceneMoveToFinalBattle
        private IEnumerator SceneMoveToFinalBattleCoroutine()
        {
            yield return null;
        }
        #endregion
    }

    
}