using System;
using _SLIME.BaseScripts;
using _SLIME.GameLoop;
using UnityEngine;
using UnityEngine.Serialization;

namespace _SLIME.Scenes.Pinball.Scripts
{
    public class PinballSceneManager: ProjectMonoBehavior
    {
        private static readonly int EndScene = Animator.StringToHash("End Scene");
        [SerializeField] private int icicleHitsToMoveOn = 3;
        [SerializeField] private Animator sceneChangeAnimator;

        private int _icicleBreakCounter = 0;
        private void OnEnable()
        {
            GameEvents.IcicleCrumbled += OnIcicleCrumbled;
            GameEvents.ResetButtonPressed += OnReset; // TODO: check if this is the meaning of the event
        }

        private void OnDisable()
        {
            GameEvents.IcicleCrumbled -= OnIcicleCrumbled;
            GameEvents.ResetButtonPressed -= OnReset;
        }

        private void OnIcicleCrumbled()
        {
            if (_icicleBreakCounter++ == icicleHitsToMoveOn)
            {
                sceneChangeAnimator.SetTrigger(EndScene);
                Invoke(nameof(LoadNextScene), 3f); // TODO: switch to amination event
            }
        }

        private void OnReset()
        {
            _icicleBreakCounter = 0;
        }
        
        private void LoadNextScene() // TODO: Animation event call
        {
            SceneLoader.LoadScene(SceneType.BossFinalBattleScene);
        }
    }
}