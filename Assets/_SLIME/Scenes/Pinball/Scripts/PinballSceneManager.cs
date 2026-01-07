using System;
using _SLIME.BaseScripts;
using _SLIME.GameLoop;
using UnityEngine;

namespace _SLIME.Scenes.Pinball.Scripts
{
    public class PinballSceneManager: ProjectMonoBehavior
    {
        [SerializeField] private int icicleHitsToMoveOn = 4;

        private int _icicleHitCounter = 0;
        private void OnEnable()
        {
            GameEvents.IcicleGotHit += OnIcicleGotHit;
            GameEvents.ResetButtonPressed += OnReset; // TODO: check if this is the meaning of the event
        }

        private void OnDisable()
        {
            GameEvents.IcicleGotHit -= OnIcicleGotHit;
            GameEvents.ResetButtonPressed -= OnReset;
        }

        private void OnIcicleGotHit()
        {
            if (++_icicleHitCounter == icicleHitsToMoveOn)
            {
                SceneLoader.LoadScene(SceneType.BossFinalBattleScene);
            }
        }

        private void OnReset()
        {
            _icicleHitCounter = 0;
        }
    }
}