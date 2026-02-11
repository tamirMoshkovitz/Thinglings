using System;
using System.Collections;
using _SLIME.Boss;
using _SLIME.Core.MenuSettings.Scripts;
using _SLIME.GameLoop;
using UnityEngine;

public class SecondPhaseState : State
{
    private readonly BaseBossConfigurations _secondPhaseConfigurations;
    private readonly BossBrain _bossBrain;
    public static event Action SecondPhaseStarted;

    public SecondPhaseState(StateMachine stateMachine, BossBrain bossBrain) : base(stateMachine)
    {
        _bossBrain = bossBrain;
        BossConfigStruct config = MenuController.gameMode switch
        {
            GameMode.Easy => bossBrain.bossConfigEasy,
            GameMode.Medium => bossBrain.bossConfigMedium,
            GameMode.Hard => bossBrain.bossConfigHard,
            _ => bossBrain.bossConfigEasy
        };
        _secondPhaseConfigurations = config.secondPhaseConfigurations;
    }
    
    public override IEnumerator Enter()
    {
        while(_bossBrain.WaterStateActivated) yield return null;
        yield return base.Enter();
        EnterHealth = _bossBrain.currentHealth;
        BossBrain.bossConfigurations = _secondPhaseConfigurations;
        _bossBrain.SavePhaseCheckpoint(BossPhaseType.SecondPhase);
        SecondPhaseStarted?.Invoke();
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
