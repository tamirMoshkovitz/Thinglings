using _SLIME.BaseScripts;
using DG.Tweening;
using UnityEngine;

namespace _SLIME.Tutorial
{
    [CreateAssetMenu(fileName = "TutorialConfig", menuName = "Scriptable Objects/TutorialConfiguration")]
    public class TutorialScriptable : TabbedScriptableObject
    {
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

        public RockShakeSettings RockShakeSettings => rockShakeSettings;
        public RockStateSet RockStateSet => rockStateSet;
        public RiseToBossStateSet RiseToBossStateSet => riseToBossStateSet;
        public SlimeConnectsStateSet SlimeConnectsStateSet => slimeConnectsStateSet;
        public SlimeTearsStateSet SlimeTearsStateSet => slimeTearsStateSet;
    }
}