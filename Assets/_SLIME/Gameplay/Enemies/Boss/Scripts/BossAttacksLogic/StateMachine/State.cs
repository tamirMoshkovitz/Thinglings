using UnityEngine;

public class State
{
    protected StateMachine StateMachine;
    
    public State(StateMachine stateMachine)
    {
        this.StateMachine = stateMachine;
    }
    
    public virtual void Enter()
    {
        Debug.Log("Entering State: " + this.GetType().Name);
        DoChecks();
    }
    
    public virtual void Exit()
    {
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
