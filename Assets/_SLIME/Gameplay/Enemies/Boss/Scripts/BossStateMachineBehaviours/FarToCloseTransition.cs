using _SLIME.Boss;
using _SLIME.Core.Audio.FMOD.Scripts;
using FMODUnity;
using UnityEngine;

public class FarToCloseTransition : BossBaseBehaviour
{
    [SerializeField] private EventReference transitionSFX;
    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        base.OnStateEnter(animator, stateInfo, layerIndex);
        Data.BossCloseState();
        SFXPlayer.Play(transitionSFX);
    }
}
