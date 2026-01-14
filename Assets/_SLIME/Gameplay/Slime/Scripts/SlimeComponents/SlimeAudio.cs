using _Slime.Audio;
using UnityEngine;

namespace _SLIME.Slime
{
    public class SlimeAudio : ISlimeBehaviorComponent
    {
        private readonly SlimeData _slimeData;
        // private SoundObject _tearSoundObject;
        // private SoundObject _stretchSoundObject;

        public SlimeAudio(SlimeData slimeData)
        {
            _slimeData = slimeData;
        }

        public void OnSlimeTears()
        {
            // _stretchSoundObject?.audioSource.Stop();
            // if (!_tearSoundObject || !_tearSoundObject.audioSource.isPlaying)
            // {
            //     // _tearSoundObject = AudioManager.Instance.Play(AudioName.SlimeTear, Vector3.zero, true); TODO: change to FMOD
            // }
        }

        public void OnSlimeConnected()
        {
            // AudioManager.Instance.Play(AudioName.SlimeConnect, Vector3.zero, true);TODO: change to FMOD
        }

        public void UpdateStretch()
        {
            // if (!_stretchSoundObject || !_stretchSoundObject.audioSource.isPlaying)
            // {
            //     // _stretchSoundObject = AudioManager.Instance.Play(AudioName.SlimeStretch, Vector3.zero);TODO: change to FMOD
            // }
            //
            // _stretchSoundObject.audioSource.pitch = _slimeData.StretchRatio;
        }

        public void OnPauseGame()
        {
            // _tearSoundObject?.audioSource.Stop();TODO: change to FMOD
            // _stretchSoundObject?.audioSource.Stop();
        }

        public void OnTearFinished()
        {
        }

        public void OnDestroy()
        {
        }

        public void OnResumeGame()
        {
        }
    }
}