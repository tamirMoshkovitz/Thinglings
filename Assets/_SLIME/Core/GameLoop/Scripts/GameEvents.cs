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
        public static Action IcicleCrumbled;
        
        public static Action WaterAttackStarted;
        public static Action WaterAttackEnded;
        
        // FMOD Events
        public static Action FmodPhaseOne;
        public static Action FmodPhaseTwo;
        public static Action FmodPhaseThree;
        public static Action FmodPhaseFour;
        public static Action FmodPhaseFive;
        
        // TODO: IN FUTURE, RESET SHOULD BE ACTIVATED ON ProjectMonoBehaviour to be applied by inheritance to all objects
        public static void RaiseResetGame()
        {
            ResetButtonPressed?.Invoke();
        }
    }
}