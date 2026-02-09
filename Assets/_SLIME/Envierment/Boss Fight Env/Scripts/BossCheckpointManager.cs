using System;
using _SLIME.GameLoop;
using UnityEngine;

namespace _SLIME.Boss
{
    public enum BossPhaseType
    {
        FirstPhase,
        SecondPhase,
        TunnelPhase
    }

    public class BossCheckpointManager : MonoBehaviour
    {
        public static BossCheckpointManager Instance { get; private set; }

        public BossPhaseType CurrentSavedPhase = BossPhaseType.FirstPhase;

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

        private void OnEnable()
        {
            GameEvents.ResetGame += ResetCheckpoint;
        }

        private void OnDisable()
        {
            GameEvents.ResetGame -= ResetCheckpoint;
        }

        public void SaveCheckpoint(BossPhaseType phase)
        {
            CurrentSavedPhase = phase;
        }

        public void ResetCheckpoint()
        {
            CurrentSavedPhase = BossPhaseType.FirstPhase;
        }
    }
}