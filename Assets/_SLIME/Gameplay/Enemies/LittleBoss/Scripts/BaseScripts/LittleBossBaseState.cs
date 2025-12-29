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
        [SerializeField] protected BaseBossSettings curSet;

        public virtual void Init(LittleBossBrain brain)
        {
            Data = brain;
            states.Add(this);
        }

        public static void UpdateSettings(BaseBossSettings newSet)
        {
            foreach (var s in states)
            {
                s.UpdateSet(newSet);
            }
        }

        public virtual void UpdateSet(BaseBossSettings newSet)
        {
            curSet = newSet;
        }

        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            base.OnStateEnter(animator, stateInfo, layerIndex);
            Logic.StartLogic();
        }

        public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            base.OnStateExit(animator, stateInfo, layerIndex);
            Logic.EndLogic();
        }
    }
}