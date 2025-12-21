using _SLIME.BaseScripts;
using UnityEngine;

namespace _SLIME.Utils
{
    /// <summary>
    /// MonoBehaviour component that provides centralized logging functionality for the game.
    /// Supports conditional logging based on an active flag and provides different log levels.
    /// </summary>
    public class GameLogger : ProjectMonoBehavior
    {
        [Header("Logging Settings")]
        [Tooltip("If false, no logs will be printed. Useful for disabling debug output in production builds.")]
        [SerializeField] private bool _active = true;
        public bool Active => _active;

        /// <summary>
        /// Logs an informational message if logging is active.
        /// </summary>
        /// <param name="message">The message to log.</param>
        public void Log(string message)
        {
            if (!_active) return;
            Debug.Log(message);
        }

        /// <summary>
        /// Logs a warning message if logging is active.
        /// </summary>
        /// <param name="message">The warning message to log.</param>
        public void LogWarning(string message)
        {
            if (!_active) return;
            Debug.LogWarning(message);
        }

        /// <summary>
        /// Logs an error message if logging is active.
        /// </summary>
        /// <param name="message">The error message to log.</param>
        public void LogError(string message)
        {
            if (!_active) return;
            Debug.LogError(message);
        }
    }
}