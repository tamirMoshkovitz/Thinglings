using System.Collections.Generic;
using _SLIME.Boss;
using UnityEngine;

namespace _SLIME.LittleBoss
{
    public abstract class LittleBossBaseState: StateMachineBehaviour
    {
        private static readonly List<LittleBossBaseState> states = new List<LittleBossBaseState>();
        protected LittleBossBrain Data;
        protected LittleBossBaseLogic Logic;
        protected BaseBossConfigurations curSet => BossBrain.bossConfigurations;

        public virtual void Init(LittleBossBrain brain)
        {
            Data = brain;
        }
        
        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            base.OnStateEnter(animator, stateInfo, layerIndex);
            Logic?.StartLogic();
        }

        public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            base.OnStateUpdate(animator, stateInfo, layerIndex);
            Logic?.UpdateLogic();
        }

        public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            base.OnStateExit(animator, stateInfo, layerIndex);
            Logic?.EndLogic();
        }
        
    }
}