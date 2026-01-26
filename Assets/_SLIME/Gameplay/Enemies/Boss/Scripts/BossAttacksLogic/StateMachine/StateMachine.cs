using UnityEngine;

public class StateMachine
{
    private readonly MonoBehaviour _monoCoroutineRunner;
    public State CurrentState { get; private set; }

    public StateMachine(MonoBehaviour monoCoroutineRunner)
    {
        _monoCoroutineRunner  = monoCoroutineRunner;
    }
    public void Initialize(State startingState)
    {
        CurrentState = startingState;
        _monoCoroutineRunner.StartCoroutine(CurrentState.Enter());
    }

    public void ChangeState(State newState)
    {
        CurrentState.Exit();
        CurrentState = newState;
        _monoCoroutineRunner.StartCoroutine(CurrentState.Enter());
    }
}
