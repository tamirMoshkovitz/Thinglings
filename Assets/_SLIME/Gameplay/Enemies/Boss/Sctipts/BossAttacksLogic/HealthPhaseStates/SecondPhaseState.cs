using _SLIME.Boss;
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
    
    public override void Enter()
    {
        base.Enter();
        _bossBrain.bossConfigurations = _secondPhaseConfigurations;
        _bossBrain.WaterStateActivated = true;
    }
    
    public override void LogicUpdate()
    {
        base.LogicUpdate();
        if (_bossBrain.currentHealth <= _secondPhaseConfigurations.PhaseSettings.upperHealthThreshold
            && _bossBrain.currentHealth > _secondPhaseConfigurations.PhaseSettings.lowerHealthThreshold)
            return;
        StateMachine.ChangeState(_bossBrain.ThirdPhaseState);
    }
}
