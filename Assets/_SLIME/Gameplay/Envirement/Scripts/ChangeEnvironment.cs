using System;
using _SLIME.Boss;
using UnityEngine;

public class ChangeEnvironment : MonoBehaviour
{
    private static readonly int EnterCloseState = Animator.StringToHash("EnterCloseState");
    private static readonly int EnterFarState = Animator.StringToHash("EnterFarState");
    
    [SerializeField] private Animator animator;

    private void OnEnable()
    {
        BossBrain.CloseState += OnCloseState;
        BossBrain.FarState += OnFarState;
    }
    
    private void OnDisable()
    {
        BossBrain.CloseState -= OnCloseState;
        BossBrain.FarState -= OnFarState;
    }

    private void OnFarState()
    {
        animator.SetTrigger(EnterFarState);
    }

    private void OnCloseState()
    {
        animator.SetTrigger(EnterCloseState);
    }
}
