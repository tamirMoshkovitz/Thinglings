using _SLIME.Core.Audio.FMOD.Scripts;
using FMODUnity;
using UnityEngine;
using UnityEngine.Animations;

namespace _SLIME.Boss
{
    public class BossDeathSFX: BossBaseBehaviour
    {
        [SerializeField] private EventReference deathSFX;
        
        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex, AnimatorControllerPlayable controller)
        {
            SFXPlayer.Play(deathSFX);
        }
    }
}