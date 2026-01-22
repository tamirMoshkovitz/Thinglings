using UnityEngine;

namespace _SLIME.Boss
{
    public enum BossPhaseType
    {
        Starting,
        FirstPhase,
        SecondPhase,
        ThirdPhase,
        TunnelPhase
    }

    public class BossCheckpointManager : MonoBehaviour
    {
        public static BossCheckpointManager Instance { get; private set; }

        public BossPhaseType CurrentSavedPhase = BossPhaseType.Starting;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        public void SaveCheckpoint(BossPhaseType phase)
        {
            CurrentSavedPhase = phase;
        }

        public void ResetCheckpoint()
        {
            CurrentSavedPhase = BossPhaseType.Starting;
        }
    }
}