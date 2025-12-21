using UnityEngine;

namespace _Slime.Audio
{
    /// <summary>
    /// Represents a single sound definition, including its clip, volume, pitch, and playback options.
    /// </summary>
    [System.Serializable]
    public class Sound
    {
        /// <summary>
        /// The unique name identifier for this sound.
        /// </summary>
        public AudioName name;
        
        /// <summary>
        /// The audio clip to play.
        /// </summary>
        public AudioClip clip;
        
        /// <summary>
        /// The playback volume (0.0 to 1.0).
        /// </summary>
        [Range(0.0f, 1.0f)]
        public float volume;
        
        /// <summary>
        /// The playback pitch (0.1 to 3.0).
        /// </summary>
        [Range(.1f, 3.0f)]
        public float pitch;
        
        /// <summary>
        /// The spatial blend (0 = 2D, 1 = 3D).
        /// </summary>
        public float spatialBlend;
    
        /// <summary>
        /// Whether the sound should loop.
        /// </summary>
        public bool loop;
        
        /// <summary>
        /// Whether the sound should stop when moving to the next scene.
        /// </summary>
        public bool stopWhenMovingToNextScene;
        
        /// <summary>
        /// Whether the sound should pause when the game is paused.
        /// </summary>
        public bool pauseOnGamePause;
    }
}