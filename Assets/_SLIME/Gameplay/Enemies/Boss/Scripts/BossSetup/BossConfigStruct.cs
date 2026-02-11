using System;

namespace _SLIME.Boss
{
    [Serializable]
    public struct BossConfigStruct
    {
        public BaseBossConfigurations firstPhaseConfigurations;
        public BaseBossConfigurations secondPhaseConfigurations;
        public BaseBossConfigurations thirdPhaseConfigurations;
    }
}