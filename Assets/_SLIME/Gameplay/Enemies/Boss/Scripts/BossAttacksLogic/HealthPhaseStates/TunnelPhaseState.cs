using System;
using System.Collections;
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
    public override IEnumerator Enter()
    {
        while(_bossBrain.WaterStateActivated) yield return null;
        yield return base.Enter();
        EnterHealth = _bossBrain.currentHealth;
        BossBrain.bossConfigurations = _tunnelBossConfiguration;
        _bossBrain.SavePhaseCheckpoint(BossPhaseType.TunnelPhase);
        TunnelPhaseStarted?.Invoke();
        
        GameEvents.FmodPhaseFive?.Invoke();
        Debug.Log("Entered Tunnel Phase State");
    }
    
    public override void LogicUpdate()
    {
        if(!active) return;
        base.LogicUpdate();
        if ( _bossBrain.currentHealth > _tunnelBossConfiguration.PhaseSettings.lowerHealthThreshold)
        {
            return;
        }

        if (!(_bossBrain.currentHealth <= 0)) return;
        _bossBrain.WaterStateActivated = true;
        BossDead?.Invoke();
    }
}
