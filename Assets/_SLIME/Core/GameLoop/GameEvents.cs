using System;

namespace _SLIME.GameLoop
{
    public static class GameEvents
    {
        public static Action SlimeTears;
        public static Action PauseGame;
        public static Action ResumeGame;
        public static Action slimeConnected;
        public static Action BrickShot;
        public static Action ResetButtonPressed;
        public static Action EnemyGotBricked; // TODO: is this only for spell encounter ot also for bullet? 
        public static Action IcicleGotHit;
        
        // TODO: IN FUTURE, RESET SHOULD BE ACTIVATED ON ProjectMonoBehaviour to be applied by inheritance to all objects
        public static void RaiseResetGame()
        {
            ResetButtonPressed?.Invoke();
        }
    }
}