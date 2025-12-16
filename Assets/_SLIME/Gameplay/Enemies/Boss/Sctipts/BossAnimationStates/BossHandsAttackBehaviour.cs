using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class BossHandsAttackBehaviour : BossBaseBehaviour
{
    [Header("State Specific Settings")]
    [SerializeField] private int smashesToPerform = 5;
    [SerializeField] private bool isHardMode = false;
    
    private static readonly int Hide = Animator.StringToHash("Hide");
    private Coroutine _smashRoutine;
    private List<HandWrapper> _leftHands;
    private List<HandWrapper> _rightHands;
    private readonly List<HandWrapper> _activeHands = new List<HandWrapper>();

    private class HandWrapper
    {
        private GameObject Root { get; set; }
        private readonly BossHandLogic _bossHandLogicScript;

        public HandWrapper(GameObject obj)
        {
            Root = obj;
            _bossHandLogicScript = obj.GetComponentInChildren<BossHandLogic>();
        }

        public void Activate(float duration)
        {
            if (_bossHandLogicScript) _bossHandLogicScript.SetDuration(duration);
            Root.SetActive(true);
        }

        public void Deactivate() => Root.SetActive(false);
    }

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        base.OnStateEnter(animator, stateInfo, layerIndex);

        _leftHands = data.leftHandSplines.Select(h => new HandWrapper(h)).ToList();
        _rightHands = data.rightHandSplines.Select(h => new HandWrapper(h)).ToList();

        DisableAllHands();
        _smashRoutine = data.StartCoroutine(SmashRoutine(animator));
    }

    private IEnumerator SmashRoutine(Animator animator)
    {
        for (int i = 0; i < smashesToPerform; i++)
        {
            _activeHands.Clear();

            if (isHardMode)
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
                hand.Activate(data.handAttackDuration);
            }

            yield return new WaitForSeconds(data.handAttackDuration);

            foreach (var hand in _activeHands)
            {
                hand.Deactivate();
            }

            if (i < smashesToPerform - 1)
            {
                yield return new WaitForSeconds(data.handCooldown);
            }
        }

        animator.SetTrigger(Hide);
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
        if (_smashRoutine != null) data.StopCoroutine(_smashRoutine);
        DisableAllHands();
    }
}