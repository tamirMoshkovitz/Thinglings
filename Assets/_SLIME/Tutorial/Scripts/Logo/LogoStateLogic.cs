using System;
using System.Collections;
using _SLIME.Core.MenuSettings.Scripts;
using UnityEngine;

namespace _SLIME.Tutorial
{
    [Serializable]
    public struct LogoStateDeps
    {
        public GameObject logo;
        public GameObject optionsRock;
    }
    
    [Serializable]
    public struct LogoStateSet
    {
       
    }

    public class LogoStateLogic : ITutorialStateLogic
    {
        private LogoStateDeps _logoStateDeps;
        private LogoStateSet _logoStateSet;
        private bool _finished;

        public LogoStateLogic(LogoStateDeps logoStateDeps, LogoStateSet logoStateSet)
        {
            _logoStateDeps = logoStateDeps;
            _logoStateSet = logoStateSet;
            Logo.JoystickMovedEnough += Finished;
        }

        private void Finished()
        {
            _finished = true;
        }

        public void OnDisable()
        {
            Logo.JoystickMovedEnough -= Finished;
        }

        public IEnumerator Start()
        {
            MenuController.gameTime = GameTime.Start;
            while (!_finished)
            {
                yield return null;
            }
            _logoStateDeps.logo.SetActive(false);
            _logoStateDeps.optionsRock.SetActive(false);
            MenuController.gameTime = GameTime.Game;
        }

    }
}
