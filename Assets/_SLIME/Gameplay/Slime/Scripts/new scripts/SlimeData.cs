using UnityEngine;

namespace _SLIME.Gameplay.Slime.Scripts.new_scripts
{
    public class SlimeData
    {
        private SlimeSide _sideA;
        private SlimeSide _sideB;
        
        public SlimeData(SlimeSide sideA, SlimeSide sideB)
        {
            _sideA = sideA;
            _sideB = sideB;
        }
        
        public bool Connected { get; set; }

        public float Distance => Mathf.Max(0, Vector3.Distance(_sideA.Position, _sideB.Position) - 1f); //TODO: REFACTOR!! the -1 is because the slime radius is 0.5f for both sides

        public bool ReachedMaxStretch => Distance >= MaxStretchDistance;
        public float StretchRatio => (Distance + 1f) / MaxStretchDistance; //Mathf.Clamp01(Distance / MaxStretchDistance);
        public float MaxStretchDistance // Physics by chatGPT thank you very much
        {
            get
            {
                // Effective (reduced) mass of two-body system
                var mEff = (SlimeMassA * SlimeMassB) / (SlimeMassA + SlimeMassB);
                
                // If frequency or mass is invalid, just fall back to the rest distance (no extra stretch).
                if (SpringFrequency <= 0f || mEff <= 0f)
                    return RestDistance;
                
                // Angular frequency (rad/s)
                var omega = 2f * Mathf.PI * SpringFrequency;
                
                // Spring stiffness k = m_eff * (2πf)^2
                var k = omega * omega * mEff;
                if (k <= 0f)
                    return RestDistance;
                
                // For identical springs in parallel, each spring sees the same extension x,
                // and it breaks when its own force reaches SpringBreakForce:
                // x_break = F_break / k
                var xBreak = SpringBreakForce / k;
                
                // Max distance between the two slime centers before any spring breaks
                return RestDistance + xBreak;
            }
        }


        #region Spring Joints Properties

        public float SlimeMassA => _sideA.Mass;

        public float SlimeMassB => _sideB.Mass;
        public int SpringJointCount { get; set; }
        public float SpringFrequency { get; set; }
        public float SpringBreakForce { get; set; }
        public float RestDistance { get; set; } = 0;

        #endregion
        
        public Vector2 TopLineConnectionPositionRight { get; set; }
        public Vector2 TopLineConnectionPositionLeft { get; set; }

        
    }
}