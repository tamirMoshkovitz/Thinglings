using System;
using Generics;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Audio
{
    /// <summary>
    /// Represents a pooled audio source that can play, pause, and stop sounds, and returns itself to the pool when finished.
    /// </summary>
    public class SoundObject : MonoBehaviour, IPoolable
    {
        [SerializeField] public AudioSource audioSource;
        private SoundPool _soundPool;
        private bool _active;
        private Action _callback;
        private bool _isPauseable;

        /// <summary>
        /// Subscribes to global pause/resume events when enabled.
        /// </summary>
        public void OnEnable()
        {
            // GameEvents.OnGameResumed += OnGameResume;
            // GameEvents.OnGamePaused += OnGamePaused;
        }

        /// <summary>
        /// Unsubscribes from global pause/resume events when disabled.
        /// </summary>
        public void OnDisable()
        {
            // GameEvents.OnGameResumed -= OnGameResume;
            // GameEvents.OnGamePaused -= OnGamePaused;
        }

        /// <summary>
        /// Pauses the audio if this sound is pauseable.
        /// </summary>
        private void OnGamePaused()
        {
            if (!_isPauseable) return;
            _active = false;
            audioSource.Pause();
        }

        /// <summary>
        /// Resumes the audio if this sound is pauseable.
        /// </summary>
        private void OnGameResume()
        {
            if (!_isPauseable) return;
            _active = true;
            audioSource.UnPause();
        }

        /// <summary>
        /// Checks if the sound has finished playing and returns it to the pool if so.
        /// </summary>
        public void Update()
        {
            if (_active && !audioSource.isPlaying)
            {
                _callback?.Invoke();
                _soundPool.Return(this);
            }
        }

        /// <summary>
        /// Plays the given sound at the specified position, with optional callback on finish.
        /// </summary>
        /// <param name="sound">The sound definition to play.</param>
        /// <param name="pos">The world position for the sound.</param>
        /// <param name="pool">The pool to return to when finished.</param>
        /// <param name="callback">Optional callback when the sound finishes.</param>
        public void Play(Sound sound, Vector3 pos, SoundPool pool,bool randomizePitch, Action callback = null)
        {
            _soundPool = pool;
            gameObject.transform.position = pos;
            audioSource.clip = sound.clip;
            audioSource.volume = sound.volume;
            audioSource.pitch =  randomizePitch? Random.Range(0.85f, 1.15f) * sound.pitch: sound.pitch;
            audioSource.loop = sound.loop;
            audioSource.spatialBlend = sound.spatialBlend;
            _isPauseable = sound.pauseOnGamePause;
            _callback = callback;
            audioSource.Play();
            _active = true;
        }

        /// <summary>
        /// Stops the sound and returns it to the pool if it is active.
        /// </summary>
        public void StopSound()
        {
            if (_active && audioSource.isPlaying)
            {
                audioSource.Stop();
                _soundPool.Return(this);
            }
        }

        /// <summary>
        /// Resets the sound object to inactive state.
        /// </summary>
        public void Reset()
        {
            _active = false;
        }
    }
}
