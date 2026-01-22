using System;
using _SLIME.Boss;
using _SLIME.GameLoop;
using Unity.VisualScripting;
using UnityEngine;

public class ThirdPhaseState : State
{
    private readonly BaseBossConfigurations _thirdPhaseConfigurations;
    private readonly BossBrain _bossBrain;

    public ThirdPhaseState(StateMachine stateMachine, BossBrain bossBrain, BaseBossConfigurations thirdPhaseConfigurations) : base(stateMachine)
    {
        _bossBrain = bossBrain;
        _thirdPhaseConfigurations = thirdPhaseConfigurations;
    }
    
    public override void Enter()
    {
        base.Enter();
        _bossBrain.bossConfigurations = _thirdPhaseConfigurations;
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
        StateMachine.ChangeState(_bossBrain.TunnelPhaseState);
    }
    
    public override void Exit()
    {
        base.Exit();
        _bossBrain.WaterStateActivated = true;
    }
}
