using _SLIME.Boss;
using _SLIME.Core.Audio.FMOD.Scripts;
using FMODUnity;
using UnityEngine;

public class CloseToOutsideState : BossBaseBehaviour
{
    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        base.OnStateEnter(animator, stateInfo, layerIndex);
        Data.BossOutsideState();
    }
}
