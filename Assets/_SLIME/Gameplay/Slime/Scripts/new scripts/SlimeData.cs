using UnityEngine;

namespace _SLIME.Gameplay.Slime.Scripts.new_scripts
{
    public class SlimeData
    {
        private SlimeSide _sideA;
        private SlimeSide _sideB;
        
        public void Initialize(SlimeSide sideA, SlimeSide sideB)
        {
            _sideA = sideA;
            _sideB = sideB;
        }

        private bool _connected = false;
        public bool Connected
        {
            get => _connected;
            set
            {
                if (_connected && !value)
                {
                    SlimeEvents.SlimeTears?.Invoke();
                }
                else if (!_connected && value)
                {
                    SlimeEvents.SlimeConnected?.Invoke();
                }
                _connected = value;
            }
        }

        public float Distance => Mathf.Max(0, Vector3.Distance(_sideA.Position, _sideB.Position) - 1f); //TODO: REFACTOR!! the -1 is because the slime radius is 0.5f for both sides

        public bool ReachedMaxStretch => Distance >= MaxStretchDistance;
        public float StretchRatio => Distance / MaxStretchDistance;
        public float MaxStretchDistance // Physics by chatGPT thank you very much
        {
            get
            {
                // Approximate single-spring break distance using a mass-spring model.
                // Distance metric in this class is (center distance - 1f), so RestDistance should be in the same metric.
                // If RestDistance was never initialized, fall back to the current Distance as the relaxed baseline.
                float rest = RestDistance > 0f ? RestDistance : Distance;

                // Reduced (effective) mass for two-body spring.
                float denom = SlimeMassA + SlimeMassB;
                if (denom <= 0f || SpringFrequency <= 0f)
                    return rest;

                float mEff = (SlimeMassA * SlimeMassB) / denom;
                if (mEff <= 0f)
                    return rest;

                // Convert frequency (Hz) to angular frequency (rad/s) and compute stiffness k ≈ mEff * (2πf)^2.
                float omega = 2f * Mathf.PI * SpringFrequency;
                float k = omega * omega * mEff;
                if (k <= 0f)
                    return rest;

                // Break when spring force F = kx reaches breakForce -> xBreak = breakForce / k.
                float xBreak = SpringBreakForce / k;

                // Max allowed distance before the joint is expected to break (same metric as Distance).
                return Mathf.Max(0f, xBreak) * 7f;
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

        public bool IsStrained => StretchRatio > .66f && Connected;
    }
}