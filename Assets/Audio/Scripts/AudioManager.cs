using System;
using System.Collections.Generic;
using _SPC.Core.BaseScripts.Generics.MonoSingletone;
using UnityEngine.SceneManagement;
using UnityEngine;

namespace Audio
{
    /// <summary>
    /// Manages all audio playback, pooling, and scene-based sound control for the game.
    /// </summary>
    public class AudioManager : MonoSingleton<AudioManager>
    {
        [SerializeField] private SoundPool soundPool;
        public Sound[] sounds;

        private Dictionary<AudioName, List<SoundObject>> soundDictionary =
            new Dictionary<AudioName, List<SoundObject>>();

        private int _lastSceneIndex = -1;

        /// <summary>
        /// Initializes the AudioManager and tracks the starting scene index.
        /// </summary>
        private void Awake()
        {
            _lastSceneIndex = SceneManager.GetActiveScene().buildIndex;
        }

        /// <summary>
        /// Checks for scene changes and stops scene-bound sounds if needed.
        /// </summary>
        private void Update()
        {
            int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
            if (currentSceneIndex != _lastSceneIndex)
            {
                _lastSceneIndex = currentSceneIndex;
                StopSceneBoundSounds();
            }
        }

        /// <summary>
        /// Stops and clears all sounds that should not persist between scenes.
        /// </summary>
        private void StopSceneBoundSounds()
        {
            foreach (var soundListKey in soundDictionary)
            {
                Sound soundDef = Array.Find(sounds, s => s.name == soundListKey.Key);
                if (soundDef != null && soundDef.stopWhenMovingToNextScene)
                {
                    foreach (var soundObj in soundListKey.Value)
                    {
                        soundObj.StopSound();
                    }

                    soundListKey.Value.Clear();
                }
            }
        }

        /// <summary>
        /// Plays a sound at the given position and returns the SoundObject instance.
        /// </summary>
        /// <param name="name">The AudioName to play.</param>
        /// <param name="pos">The world position for the sound.</param>
        /// <param name="callback">Optional callback when the sound finishes.</param>
        /// <returns>The SoundObject instance playing the sound.</returns>
        public SoundObject Play(AudioName name, Vector3 pos, bool ramdomizePitch = false, Action callback = null)
        {
            Sound soundDef = Array.Find(sounds, sound => sound.name == name);
            if (soundDef == null)
            {
                Debug.LogWarning($"Sound: {name} not found!");
                return null;
            }

            SoundObject soundObject = soundPool.Get();
            soundDictionary.TryAdd(name, new List<SoundObject>());
            soundDictionary[name].Add(soundObject);
            soundObject.Play(soundDef, pos, soundPool, ramdomizePitch, callback);
            return soundObject;
        }

        /// <summary>
        /// Stops all sounds with the given AudioName.
        /// </summary>
        /// <param name="name">The AudioName to stop.</param>
        public void Stop(AudioName name)
        {
            if (soundDictionary.TryGetValue(name, out List<SoundObject> soundObjects))
            {
                foreach (var sound in soundObjects)
                {
                    sound.StopSound();
                }

                soundObjects.Clear();
            }
        }

        /// <summary>
        /// Stops and clears all currently playing sounds.
        /// </summary>
        public void StopAll()
        {
            foreach (var soundListKey in soundDictionary)
            {
                foreach (var sound in soundListKey.Value)
                {
                    sound.StopSound();
                }
            }

            soundDictionary.Clear();
        }
    }

    /// <summary>
    /// Enum of all named audio clips in the game.
    /// </summary>
    public enum AudioName
    {
        SlimeTear,
        SlimeStretch
    }
}