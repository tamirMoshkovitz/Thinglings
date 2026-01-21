using _Slime.Audio;
using UnityEngine;

namespace _SLIME.Slime
{
    public class SlimeAudio : ISlimeBehaviorComponent
    {
        private readonly SlimeData _slimeData;
        private ControlledSfx _slimeStretchSfx;

        public SlimeAudio(SlimeData slimeData, ControlledSfx slimeStretchSfx)
        {
            _slimeData = slimeData;
            _slimeStretchSfx = slimeStretchSfx;
        }

        public void OnSlimeTears()
        {
            _slimeStretchSfx.SetParameter("stretch amount", 1f);
        }

        public void OnSlimeConnected()
        {
            _slimeStretchSfx.Play();
            _slimeStretchSfx.SetParameter("stretch amount", 0f);
            Debug.Log("Playing stretch SFX");
        }

        public void UpdateStretch()
        {
            _slimeStretchSfx.SetParameter("stretch amount", _slimeData.StretchRatio);
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