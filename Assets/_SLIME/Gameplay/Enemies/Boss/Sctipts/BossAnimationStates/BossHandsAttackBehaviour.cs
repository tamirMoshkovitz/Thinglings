using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Serialization;


namespace _SLIME.Boss
{
    public class BossHandsAttackBehaviour : BossBaseBehaviour
    {
        
        
        private static readonly int AttackFinished = Animator.StringToHash("AttackFinished");
        private Coroutine _smashRoutine;
        private List<HandWrapper> _leftHands;
        private List<HandWrapper> _rightHands;
        private readonly List<HandWrapper> _activeHands = new List<HandWrapper>();

        private class HandWrapper
        {
            private GameObject Root { get; set; }
            private readonly BossHandAttackLogic _bossHandAttackLogicScript;

            public HandWrapper(GameObject obj)
            {
                Root = obj;
                _bossHandAttackLogicScript = obj.GetComponentInChildren<BossHandAttackLogic>();
            }

            public void Activate(float duration)
            {
                if (_bossHandAttackLogicScript) _bossHandAttackLogicScript.SetDuration(duration);
                Root.SetActive(true);
            }

            public void Deactivate() => Root.SetActive(false);
        }

        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            base.OnStateEnter(animator, stateInfo, layerIndex);

            _leftHands = Data.leftHandSplines.Select(h => new HandWrapper(h)).ToList();
            _rightHands = Data.rightHandSplines.Select(h => new HandWrapper(h)).ToList();

            DisableAllHands();
            _smashRoutine = Data.StartCoroutine(SmashRoutine(animator));
        }

        private IEnumerator SmashRoutine(Animator animator)
        {
            for (int i = 0; i < Data.bossSettings.HandsAttack.totalHandsToUse; i++)
            {
                _activeHands.Clear();

                if (Data.bossSettings.HandsAttack.useBothHands)
                {
                    AddRandomHand(_leftHands);
                    AddRandomHand(_rightHands);
                }
                else
                {
                    bool isLeft = Random.value > 0.5f;
                    AddRandomHand(isLeft ? _leftHands : _rightHands);
                }

                foreach (var hand in _activeHands)
                {
                    hand.Activate(Data.bossSettings.HandsAttack.handAttackDuration);
                }

                yield return new WaitForSeconds(Data.bossSettings.HandsAttack.handAttackDuration);

                foreach (var hand in _activeHands)
                {
                    hand.Deactivate();
                }

                if (i < Data.bossSettings.HandsAttack.totalHandsToUse - 1)
                {
                    yield return new WaitForSeconds(Data.bossSettings.HandsAttack.handCooldown);
                }
            }

            animator.SetTrigger(AttackFinished);
        }

        private void AddRandomHand(List<HandWrapper> list)
        {
            if (list == null || list.Count == 0) return;
            _activeHands.Add(list[Random.Range(0, list.Count)]);
        }

        private void DisableAllHands()
        {
            _leftHands?.ForEach(h => h.Deactivate());
            _rightHands?.ForEach(h => h.Deactivate());
        }

        public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            if (_smashRoutine != null) Data.StopCoroutine(_smashRoutine);
            DisableAllHands();
        }
    }
}