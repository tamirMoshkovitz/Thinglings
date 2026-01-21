using System;
using _SLIME.Boss;
using _SLIME.GameLoop;
using Unity.VisualScripting;
using UnityEngine;

public class ThirdPhaseState : State
{
    private readonly BaseBossConfigurations _thirdPhaseConfigurations;
    private readonly BossBrain _bossBrain;
    public static event Action TunnelPhaseStarted;
    private bool _hasInvokedTunnelStart;

    public ThirdPhaseState(StateMachine stateMachine, BossBrain bossBrain, BaseBossConfigurations thirdPhaseConfigurations) : base(stateMachine)
    {
        _bossBrain = bossBrain;
        _thirdPhaseConfigurations = thirdPhaseConfigurations;
    }
    
    public override void Enter()
    {
        base.Enter();
        _bossBrain.bossConfigurations = _thirdPhaseConfigurations;
        _bossBrain.WaterStateActivated = true;
        _bossBrain.SavePhaseCheckpoint(BossPhaseType.ThirdPhase);
        // GameEvents.FmodPhaseFour?.Invoke();
    }
    
    public override void LogicUpdate()
    {
        base.LogicUpdate();
        if (_bossBrain.currentHealth <= _thirdPhaseConfigurations.PhaseSettings.upperHealthThreshold
            && _bossBrain.currentHealth > _thirdPhaseConfigurations.PhaseSettings.lowerHealthThreshold)
        {
            return;
        }

        if (!_hasInvokedTunnelStart)
        {
            Debug.Log("Invoking Tunnel Phase Start");
            TunnelPhaseStarted?.Invoke();
            _hasInvokedTunnelStart = true;
            
            GameEvents.FmodPhaseFour?.Invoke();
        }
    }
    
    
}
