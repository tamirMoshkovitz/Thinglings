using System;

public static class GameEvents
{
    public static Action SlimeTears;
    public static Action PauseGame;
    public static Action ResumeGame;
    public static Action slimeConnected;
    public static Action BrickShot;
    public static Action ResetButtonPressed;
    public static Action EnemyGotBricked;
    
    public static void RaiseResetGame()
    {
        ResetButtonPressed?.Invoke();
    }
}