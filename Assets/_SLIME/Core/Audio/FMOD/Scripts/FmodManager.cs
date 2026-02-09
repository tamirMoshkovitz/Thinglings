using System;
using System.Collections;
using _Slime.Audio;
using _SLIME.BaseScripts;
using _SLIME.GameLoop;
using FMOD.Studio;
using FMODUnity;
using UnityEngine;
using STOP_MODE = FMOD.Studio.STOP_MODE;

public class FmodManager : ProjectMonoBehavior
{
    [SerializeField] private GameObject phaseOnePlaster;
    [SerializeField] private GameObject phaseTwoPlaster;
    [SerializeField] private GameObject phaseThreePlaster;
    [SerializeField] private GameObject phaseFourPlaster;
    [SerializeField] private GameObject phaseFivePlaster;
    [SerializeField] private GameObject phaseSixPlaster;
    
    public static FmodManager Instance { get; private set; }
    
    private StudioEventEmitter _musicEmitter;
    
    void Awake()
    {
        if (Instance) { Destroy(gameObject); return; }
        Instance = this;
        _musicEmitter = GetComponent<StudioEventEmitter>();
    }

    private void OnEnable()
    {
        GameEvents.FmodPhaseOne += SetBossPhaseOne;
        GameEvents.FmodPhaseTwo += SetBossPhaseTwo;
        GameEvents.FmodPhaseThree += SetBossPhaseThree;
        GameEvents.FmodPhaseFour += SetBossPhaseFour;
        GameEvents.FmodPhaseFive += SetBossPhaseFive;
        GameEvents.FmodPhaseSix += SetBossPhaseSix;
        
        GameEvents.WaterAttackStarted += OnWaterAttackStarted;
        GameEvents.WaterAttackEnded += OnWaterAttackEnded;

        GameEvents.ResetGame += OnResetGame;
    }
    
    private void OnDisable()
    {
        GameEvents.FmodPhaseOne -= SetBossPhaseOne;
        GameEvents.FmodPhaseTwo -= SetBossPhaseTwo;
        GameEvents.FmodPhaseThree -= SetBossPhaseThree;
        GameEvents.FmodPhaseFour -= SetBossPhaseFour;
        GameEvents.FmodPhaseFive -= SetBossPhaseFive;
        GameEvents.FmodPhaseSix -= SetBossPhaseSix;
        
        GameEvents.WaterAttackStarted -= OnWaterAttackStarted;
        GameEvents.WaterAttackEnded -= OnWaterAttackEnded;

        GameEvents.ResetGame -= OnResetGame;
    }

    private void SetBossPhaseOne()
    {
        phaseOnePlaster.SetActive(true);
        phaseOnePlaster.SetActive(false);
    }
    
    private void SetBossPhaseTwo()
    {
        phaseTwoPlaster.SetActive(true);
        phaseTwoPlaster.SetActive(false);
    }
    
    private void SetBossPhaseThree()
    {
        phaseThreePlaster.SetActive(true);
        phaseThreePlaster.SetActive(false);
    }
    
    private void SetBossPhaseFour()
    {
        phaseFourPlaster.SetActive(true);
        phaseFourPlaster.SetActive(false);
    }
    
    private void SetBossPhaseFive()
    {
        phaseFivePlaster.SetActive(true);
        phaseFivePlaster.SetActive(false);
    }
    
    private void SetBossPhaseSix()
    {
        phaseSixPlaster.SetActive(true);
        phaseSixPlaster.SetActive(false);
    }
    
    private void OnWaterAttackStarted()
    {
        StartCoroutine(InterpolateParameter(_musicEmitter.EventInstance, "water checkpoint", 1f, 1f));
    }
    
    private void OnWaterAttackEnded()
    {
        StopCoroutine(InterpolateParameter(_musicEmitter.EventInstance, "water checkpoint", 1f, 1f));
        StartCoroutine(InterpolateParameter(_musicEmitter.EventInstance, "water checkpoint", 0f, .25f));
    }
    
    private IEnumerator InterpolateParameter(EventInstance eventInstance, string parameterName, float targetValue, float duration)
    {
        float currentTime = 0f;
        float initialValue;
        eventInstance.getParameterByName(parameterName, out initialValue);
        
        while (currentTime < duration)
        {
            currentTime += Time.deltaTime;
            float newValue = Mathf.Lerp(initialValue, targetValue, currentTime / duration);
            eventInstance.setParameterByName(parameterName, newValue);
            yield return null;
        }
        
        eventInstance.setParameterByName(parameterName, targetValue);
    }

    private void OnResetGame()
    {
        ResetAllParameters();
        _musicEmitter.EventInstance.stop(STOP_MODE.IMMEDIATE);
        _musicEmitter.EventInstance.start();
    }

    private void ResetAllParameters()
    {
        RuntimeManager.StudioSystem.setParameterByNameWithLabel("battle scenes", "pinball");
        RuntimeManager.StudioSystem.setParameterByName("first scene tension", 0f);
        RuntimeManager.StudioSystem.setParameterByName("light", 0f);
    }
}
