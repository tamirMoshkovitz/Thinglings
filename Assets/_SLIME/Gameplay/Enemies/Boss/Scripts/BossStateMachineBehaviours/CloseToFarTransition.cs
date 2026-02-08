using _SLIME.Boss;
using _SLIME.Core.Audio.FMOD.Scripts;
using FMODUnity;
using UnityEngine;

public class CloseToFarTransition : BossBaseBehaviour
{
    [SerializeField] private EventReference transitionSFX;
    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        base.OnStateEnter(animator, stateInfo, layerIndex); 
        SFXPlayer.Play(transitionSFX);
    }

    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        base.OnStateExit(animator, stateInfo, layerIndex);
        Data.BossFarState();
    }
}
