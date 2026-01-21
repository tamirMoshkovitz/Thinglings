using _SLIME.Boss;
using _SLIME.GameLoop;
using UnityEngine;

public class FirstPhaseState : State
{
    private readonly BaseBossConfigurations _firstPhaseConfigurations;
    private readonly BossBrain _bossBrain;

    public FirstPhaseState(StateMachine stateMachine, BossBrain bossBrain, BaseBossConfigurations firstPhaseConfigurations) : base(stateMachine)
    {
        _bossBrain = bossBrain;
        _firstPhaseConfigurations = firstPhaseConfigurations;
    }
    
    public override void Enter()
    {
        base.Enter();
        _bossBrain.bossConfigurations = _firstPhaseConfigurations;
        _bossBrain.SavePhaseCheckpoint(BossPhaseType.FirstPhase);
    }
    
    public override void LogicUpdate()
    {
        base.LogicUpdate();
        if (_bossBrain.currentHealth <= _firstPhaseConfigurations.PhaseSettings.upperHealthThreshold
            && _bossBrain.currentHealth > _firstPhaseConfigurations.PhaseSettings.lowerHealthThreshold)
            return;
        StateMachine.ChangeState(_bossBrain.SecondPhaseState);
    }
    
    public override void Exit()
    {
        base.Exit();
        _bossBrain.WaterStateActivated = true;
    }
}
