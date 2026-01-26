using System.Collections;
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
    
    public override IEnumerator Enter()
    {
        while(_bossBrain.WaterStateActivated) yield return null;
        yield return base.Enter();
        
        EnterHealth = _bossBrain.currentHealth;
        BossBrain.bossConfigurations = _firstPhaseConfigurations;
        _bossBrain.SavePhaseCheckpoint(BossPhaseType.FirstPhase);
        
    }
    
    public override void LogicUpdate()
    {
        if(!active) return;
        base.LogicUpdate();
        if (_bossBrain.currentHealth > _firstPhaseConfigurations.PhaseSettings.lowerHealthThreshold)
            return;
        StateMachine.ChangeState(_bossBrain.SecondPhaseState);
    }
    
    public override void Exit()
    {
        base.Exit();
        _bossBrain.WaterStateActivated = true;
    }
}
