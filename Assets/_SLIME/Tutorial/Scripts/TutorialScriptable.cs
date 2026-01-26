using _SLIME.BaseScripts;
using DG.Tweening;
using UnityEngine;

namespace _SLIME.Tutorial
{
    [CreateAssetMenu(fileName = "TutorialConfig", menuName = "Scriptable Objects/TutorialConfiguration")]
    public class TutorialScriptable : TabbedScriptableObject
    {
        [Tab("Logo")]
        [SerializeField] private LogoStateSet logoStateSet;
        
        [Tab("Rock")]
        [SerializeField] private RockShakeSettings rockShakeSettings;
        [Tab("Rock")]
        [SerializeField] private RockStateSet rockStateSet;
        
        [Tab("RiseToBoss")]
        [SerializeField] private RiseToBossStateSet riseToBossStateSet;
        
        [Tab("SlimeConnects")]
        [SerializeField] private SlimeConnectsStateSet slimeConnectsStateSet;
        
        [Tab("SlimeTears")]
        [SerializeField] private SlimeTearsStateSet slimeTearsStateSet;
        
        [Tab("SpellHit")]
        [SerializeField] private SpellHitStateSet spellHitStateSet;
        
        [Tab("LearnSlimeToConnect")]
        [SerializeField] private LearnSlimeToConnectStateSet learnSlimeToConnectStateSet;
        
        [Tab("LearnSlimeAboutSpells")]
        [SerializeField] private LearnSlimeAboutSpellsStateSet learnSlimeAboutSpellsStateSet;
        
        [Tab("BossThrowsSpell")]
        [SerializeField] private BossThrowsSpellStateSet bossThrowsSpellStateSet;
        
        [Tab("CaveShake")]
        [SerializeField] private CaveShakeStateSet caveShakeStateSet;
        
        [Tab("SceneMoveToFinalBattle")]
        [SerializeField] private SceneMoveToFinalBattleStateSet sceneMoveToFinalBattleStateSet;
        
        [Tab("BossThrowsSpell")]
        [SerializeField] private TutorialBossFlashSettings tutorialBossFlashSettings;
        
        [Tab("LogoSettings")]
        [SerializeField] private LogoSettings logoSettings;
        public RockShakeSettings RockShakeSettings => rockShakeSettings;
        public TutorialBossFlashSettings TutorialBossFlashSettings => tutorialBossFlashSettings;
        public LogoStateSet LogoStateSet => logoStateSet;
        public RockStateSet RockStateSet => rockStateSet;
        public RiseToBossStateSet RiseToBossStateSet => riseToBossStateSet;
        public SlimeConnectsStateSet SlimeConnectsStateSet => slimeConnectsStateSet;
        public SlimeTearsStateSet SlimeTearsStateSet => slimeTearsStateSet;
        public SpellHitStateSet SpellHitStateSet => spellHitStateSet;
        public LearnSlimeToConnectStateSet LearnSlimeToConnectStateSet => learnSlimeToConnectStateSet;
        public LearnSlimeAboutSpellsStateSet LearnSlimeAboutSpellsStateSet => learnSlimeAboutSpellsStateSet;
        public BossThrowsSpellStateSet BossThrowsSpellStateSet => bossThrowsSpellStateSet;
        public CaveShakeStateSet CaveShakeStateSet => caveShakeStateSet;
        public SceneMoveToFinalBattleStateSet SceneMoveToFinalBattleStateSet => sceneMoveToFinalBattleStateSet;
        public LogoSettings LogoSettings => logoSettings;
    }
}