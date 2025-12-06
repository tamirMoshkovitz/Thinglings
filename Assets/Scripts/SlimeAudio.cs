using System;
using Audio;
using Player.Interfaces;
using Unity.VisualScripting;
using UnityEngine;

public class SlimeAudio : ISlimeBehaviorComponent
{
    private SoundObject _tearSoundObject;
    private SoundObject _stretchSoundObject;
    private SlimeData _slimeData;
    
    public ISlimeBehaviorComponent Awake(SlimeData slimeData)
    {
        _slimeData = slimeData;
        return this;
    }

    // ReSharper disable Unity.PerformanceAnalysis
    public void OnSlimeTears()
    {
        _stretchSoundObject?.audioSource.Stop();
        if (!_tearSoundObject ||  !_tearSoundObject.audioSource.isPlaying)
        {
            _tearSoundObject = AudioManager.Instance.Play(AudioName.SlimeTear, Vector3.zero, true);
        }
    }

    // ReSharper disable Unity.PerformanceAnalysis
    public void OnSlimeConnected()
    {
        AudioManager.Instance.Play(AudioName.SlimeConnect, Vector3.zero, true);
    }

    // ReSharper disable Unity.PerformanceAnalysis
    public void UpdateStretch()
    {
        if (!_stretchSoundObject || !_stretchSoundObject.audioSource.isPlaying)
        {
            _stretchSoundObject = AudioManager.Instance.Play(AudioName.SlimeStretch, Vector3.zero);
        }

        _stretchSoundObject.audioSource.pitch = _slimeData?.StretchRatio ?? 1f;
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