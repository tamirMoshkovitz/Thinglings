using System.Collections;
using _SLIME.Boss;
using _SLIME.GameLoop;
using UnityEngine;

public class SecondPhaseState : State
{
    private readonly BaseBossConfigurations _secondPhaseConfigurations;
    private readonly BossBrain _bossBrain;

    public SecondPhaseState(StateMachine stateMachine, BossBrain bossBrain, BaseBossConfigurations secondPhaseConfigurations) : base(stateMachine)
    {
        _bossBrain = bossBrain;
        _secondPhaseConfigurations = secondPhaseConfigurations;
    }
    
    public override IEnumerator Enter()
    {
        while(_bossBrain.WaterStateActivated) yield return null;
        yield return base.Enter();
        EnterHealth = _bossBrain.currentHealth;
        BossBrain.bossConfigurations = _secondPhaseConfigurations;
        _bossBrain.SavePhaseCheckpoint(BossPhaseType.SecondPhase);
    }
    
    public override void LogicUpdate()
    {
        if(!active) return;
        base.LogicUpdate();
        if (_bossBrain.currentHealth > _secondPhaseConfigurations.PhaseSettings.lowerHealthThreshold)
            return;
        StateMachine.ChangeState(_bossBrain.TunnelPhaseState);
    }
    
    public override void Exit()
    {
        base.Exit();
        _bossBrain.WaterStateActivated = true;
    }
}
