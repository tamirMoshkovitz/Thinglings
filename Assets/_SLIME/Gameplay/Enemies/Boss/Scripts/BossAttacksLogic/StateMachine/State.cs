using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class State
{
    protected StateMachine StateMachine;
    public float EnterHealth;
    protected bool active = false;
    
    public State(StateMachine stateMachine)
    {
        this.StateMachine = stateMachine;
    }
    
    // this is enumerator because we dont wont the phase to start before water attack finished
    public virtual IEnumerator Enter()
    {
        active = true;
        DoChecks();
        yield break;
    }
    
    public virtual void Exit()
    {
        active = false;
    }
    
    public virtual void LogicUpdate()
    {
        DoChecks();
    }
    
    public virtual void PhysicsUpdate()
    {
        DoChecks();
    }
    
    public virtual void DoChecks()
    {
    }
    
    public virtual void AnimationTrigger()
    {
    }
}
