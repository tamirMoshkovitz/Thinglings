using _SLIME.Boss;
using UnityEngine;

public class StartingPhaseState : State
{
    private readonly BossBrain _bossBrain;
    private readonly BaseBossConfigurations _startingPhaseConfigurations;

    public StartingPhaseState(StateMachine stateMachine, BossBrain bossBrain, BaseBossConfigurations startingPhaseConfigurations) : base(stateMachine)
    {
        _bossBrain = bossBrain;
        _startingPhaseConfigurations = startingPhaseConfigurations;
    }
    
    public override void Enter()
    {
        base.Enter();
        _bossBrain.bossConfigurations = _startingPhaseConfigurations;
    }
    
    public override void LogicUpdate()
    {
        base.LogicUpdate();
        if (_bossBrain.currentHealth <= _bossBrain.bossConfigurations.PhaseSettings.upperHealthThreshold
            && _bossBrain.currentHealth > _bossBrain.bossConfigurations.PhaseSettings.lowerHealthThreshold)
            return;
        StateMachine.ChangeState(_bossBrain.FirstPhaseState);
    }
    
    public override void Exit()
    {
        base.Exit();
        _bossBrain.WaterStateActivated = true;

    }
}
