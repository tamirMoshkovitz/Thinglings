using _SLIME.Core.MenuSettings.Scripts;
using UnityEngine.InputSystem;

namespace _SLIME.Core.ControllerRumble.Scripts
{
    public static class GamepadWrapper
    {

        public static bool isEndScene;
        public static void ResetHaptics()
        {
            Gamepad.current?.ResetHaptics();
        }

        public static void SetMotorSpeeds(float lowFrequency, float highFrequency)
        {
            if (MenuController.IsGamePaused || isEndScene)
            {
                Gamepad.current?.SetMotorSpeeds(0f, 0f);
                return;
            }
            Gamepad.current?.SetMotorSpeeds(lowFrequency, highFrequency);
        }

        public static void ResumeHaptics()
        {
            Gamepad.current?.ResumeHaptics();
        }
    }
}