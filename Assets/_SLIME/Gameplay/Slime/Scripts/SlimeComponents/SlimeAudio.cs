using System;
using _SLIME.Gameplay.Slime.Scripts.new_scripts;
using Audio;
using Player.Interfaces;
using Unity.VisualScripting;
using UnityEngine;

public class SlimeAudio : ISlimeBehaviorComponent
{
    private readonly SlimeData _slimeData;
    private SoundObject _tearSoundObject;
    private SoundObject _stretchSoundObject;

    public SlimeAudio(SlimeData slimeData)
    {
        _slimeData = slimeData;
    }

    public void OnSlimeTears()
    {
        _stretchSoundObject?.audioSource.Stop();
        if (!_tearSoundObject ||  !_tearSoundObject.audioSource.isPlaying)
        {
            _tearSoundObject = AudioManager.Instance.Play(AudioName.SlimeTear, Vector3.zero, true);
        }
    }

    public void OnSlimeConnected()
    {
        AudioManager.Instance.Play(AudioName.SlimeConnect, Vector3.zero, true);
    }

    public void UpdateStretch()
    {
        if (!_stretchSoundObject || !_stretchSoundObject.audioSource.isPlaying)
        {
            _stretchSoundObject = AudioManager.Instance.Play(AudioName.SlimeStretch, Vector3.zero);
        }

        _stretchSoundObject.audioSource.pitch = _slimeData.StretchRatio;
    }

    public void OnPauseGame()
    {
        _tearSoundObject?.audioSource.Stop();
        _stretchSoundObject?.audioSource.Stop();
    }

    public void OnTearFinished() { }

    public void OnDestroy() { }

    public void OnResumeGame() { }
}