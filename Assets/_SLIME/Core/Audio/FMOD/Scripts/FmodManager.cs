using System;
using _Slime.Audio;
using _SLIME.BaseScripts;
using _SLIME.GameLoop;
using FMOD.Studio;
using FMODUnity;
using UnityEngine;

public class FmodManager : ProjectMonoBehavior
{
    [SerializeField] private GameObject phaseOnePlaster;
    [SerializeField] private GameObject phaseTwoPlaster;
    [SerializeField] private GameObject phaseThreePlaster;
    [SerializeField] private GameObject phaseFourPlaster;
    
    public static FmodManager Instance { get; private set; }
    
    void Awake()
    {
        if (Instance) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void OnEnable()
    {
        GameEvents.FmodPhaseOne += SetBossPhaseOne;
        GameEvents.FmodPhaseTwo += SetBossPhaseTwo;
        GameEvents.FmodPhaseThree += SetBossPhaseThree;
        GameEvents.FmodPhaseFour += SetBossPhaseFour;
    }
    
    private void OnDisable()
    {
        GameEvents.FmodPhaseOne -= SetBossPhaseOne;
        GameEvents.FmodPhaseTwo -= SetBossPhaseTwo;
        GameEvents.FmodPhaseThree -= SetBossPhaseThree;
        GameEvents.FmodPhaseFour -= SetBossPhaseFour;
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
}
