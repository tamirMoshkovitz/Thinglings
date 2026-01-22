using System;
using _SLIME.Boss;
using _SLIME.GameLoop;
using UnityEngine;

public class TunnelPhaseState : State
{
    private readonly BaseBossConfigurations _tunnelBossConfiguration;
    private readonly BossBrain _bossBrain;
    public static event Action TunnelPhaseStarted;
    public static event Action BossDead;
    private bool _hasInvokedTunnelStart;
    
    public TunnelPhaseState(StateMachine stateMachine, BossBrain bossBrain, BaseBossConfigurations thirdPhaseConfigurations) : base(stateMachine)
    {
        _bossBrain = bossBrain;
        _tunnelBossConfiguration = thirdPhaseConfigurations;
    }
    public override void Enter()
    {
        base.Enter();
        _bossBrain.bossConfigurations = _tunnelBossConfiguration;
        _bossBrain.SavePhaseCheckpoint(BossPhaseType.TunnelPhase);
        TunnelPhaseStarted?.Invoke();
        
        GameEvents.FmodPhaseFour?.Invoke();
    }
    
    public override void LogicUpdate()
    {
        base.LogicUpdate();
        if (_bossBrain.currentHealth <= _tunnelBossConfiguration.PhaseSettings.upperHealthThreshold
            && _bossBrain.currentHealth > _tunnelBossConfiguration.PhaseSettings.lowerHealthThreshold)
        {
            return;
        }

        if (!(_bossBrain.currentHealth <= 0)) return;
        _bossBrain.WaterStateActivated = true;
        BossDead?.Invoke();
    }
}
