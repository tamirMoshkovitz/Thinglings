using System;
using System.Collections;
using System.Collections.Generic;
using _SLIME.BaseScripts;
using UnityEngine;
using UnityEngine.Splines;
using NaughtyAttributes;

namespace _SLIME.LittleBoss
{
    [Serializable]
    public struct LittleBossMovementSettings
    {
        [BigHeader("Visualization", HeaderColor.Green)]
        [Header("Animation and Movement Settings")] 
        public float duration;
        public float waitUntilNextLoop;
        public AnimationCurve easeCurve;
        
        [Header("Rotation Settings")]
        public bool useRefinedRotation;
        public Vector3 startRotation;
        public Vector3 endRotation;
        
        [BigHeader("GamePlay", HeaderColor.Green)]
        [Header("Change To Attack Settings")]
        [MinMaxSlider(0f, 1f)]
        public Vector2 chanceToAttack;
        public AnimationCurve chanceToAttackCurve;
        public float timeToMaxChance;
        
    }
    
    
    [Serializable]
    public struct LittleBossMovementRef
    {
        public SplineAnimate splineAnimate;
        public Transform littleBossTransform;
        public Animator animator;
    }
    
    public class LittleBossMovementLogic: LittleBossBaseLogic
    {
        private static readonly int Attack = Animator.StringToHash("Attack");
        public LittleBossMovementSettings Set;
        private readonly LittleBossMovementRef _ref;
        private readonly ProjectMonoBehavior _mono;
        private List<Coroutine> _cors = new List<Coroutine>();
        private float _timerForMove = 0f;
        private float _timeInMoveState = 0f;
        private float _currentChance = 0f;
        
        public LittleBossMovementLogic(LittleBossMovementSettings set, 
            LittleBossMovementRef reference, ProjectMonoBehavior mono)
        {
            Set = set;
            _ref = reference;
            _mono = mono;
            _currentChance = Set.chanceToAttack.x;
        }
        
        public void StartLogic()
        {
            _cors.Add(_mono.StartCoroutine(AnimateMoveAndRotate()));
            _cors.Add(_mono.StartCoroutine(MoveToAttackControl()));
        }

        private IEnumerator MoveToAttackControl()
        {
            while (true)
            {
                _timeInMoveState += Time.deltaTime;

                float t = Mathf.Clamp01(_timeInMoveState / Set.timeToMaxChance);
                float curveValue = Set.chanceToAttackCurve.Evaluate(t);
                _currentChance = Mathf.Lerp(Set.chanceToAttack.x, Set.chanceToAttack.y, curveValue);
                
                if (UnityEngine.Random.value < _currentChance * Time.deltaTime)
                {
                    PerformTransitionToAttack();
                    yield break; 
                }

                yield return null;
            }
        }

        private void PerformTransitionToAttack()
        {
            _ref.animator.SetTrigger(Attack);
        }

        public void EndLogic()
        {
            foreach (var c in _cors) _mono.StopCoroutine(c);
            _timeInMoveState = 0f;
            _cors.Clear();
        }

        private IEnumerator AnimateMoveAndRotate()
        {
            while (true)
            {
                Quaternion rotationFrom = Quaternion.Euler(Set.startRotation);
                Quaternion rotationTo = Quaternion.Euler(Set.endRotation);

                while (_timerForMove < Set.duration)
                {
                    _timerForMove += Time.deltaTime;
                    float progress = _timerForMove / Set.duration;
                    float easedProgress = Set.easeCurve.Evaluate(progress);

                    _ref.splineAnimate.NormalizedTime = easedProgress;

                    if (Set.useRefinedRotation)
                    {
                        _ref.littleBossTransform.rotation = Quaternion.Slerp(rotationFrom, rotationTo, easedProgress);
                    }

                    yield return null;
                }
                _ref.splineAnimate.NormalizedTime = 1f;
                _timerForMove = 0f;
                if (Set.useRefinedRotation) _ref.littleBossTransform.rotation = rotationTo;
                yield return new WaitForSeconds(Set.waitUntilNextLoop);
            }
            // ReSharper disable once IteratorNeverReturns
        }
        
    }
}