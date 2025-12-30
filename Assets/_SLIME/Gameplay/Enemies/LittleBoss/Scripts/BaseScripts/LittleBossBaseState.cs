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
        [SerializeField] protected BaseBossConfigurations curSet;

        public virtual void Init(LittleBossBrain brain)
        {
            Data = brain;
            states.Add(this);
        }

        public static void UpdateSettings(BaseBossConfigurations newSet)
        {
            foreach (var s in states)
            {
                s.UpdateSet(newSet);
            }
        }

        public virtual void UpdateSet(BaseBossConfigurations newSet) { curSet = newSet; }
        public static void ClearStates() { states.Clear(); }

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

        public void OnDestroy() { Logic.EndLogic(); }
    }
}