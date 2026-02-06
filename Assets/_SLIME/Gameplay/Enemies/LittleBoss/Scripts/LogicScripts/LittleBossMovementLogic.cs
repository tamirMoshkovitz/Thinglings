using System;
using _SLIME.BaseScripts;
using UnityEngine;

namespace _SLIME.LittleBoss
{
    [Serializable]
    public struct LittleBossMovementSettings
    {
        public float duration;
        public AnimationCurve easeCurve;
    }

    [Serializable]
    public struct LittleBossMovementRef
    {
        public Transform rootTransform;
        public Transform movePoint;
        public Animator animator;
    }

    public class LittleBossMovementLogic : LittleBossBaseLogic
    {
        private static readonly int FinishedMove = Animator.StringToHash("FinishedMove");
        public LittleBossMovementSettings Set;
        private readonly LittleBossMovementRef _ref;

        private float _timer;
        private Vector3 _startPos;
        private Vector3 _targetPos;
        private float _duration;
        private bool _active;

        public LittleBossMovementLogic(LittleBossMovementSettings set,
            LittleBossMovementRef reference)
        {
            Set = set;
            _ref = reference;
        }

        public void StartLogic()
        {
            _duration = Mathf.Max(0.01f, Set.duration);
            _timer = 0f;
            _startPos = _ref.rootTransform.position;
            _targetPos = _ref.movePoint.position;
            _targetPos.z = _startPos.z;
            _active = true;
        }

        public void EndLogic()
        {
            _active = false;
        }

        public void UpdateLogic()
        {
            if (!_active) return;

            _timer += Time.deltaTime;
            float progress = Mathf.Clamp01(_timer / _duration);
            float easedProgress = Set.easeCurve != null ? Set.easeCurve.Evaluate(progress) : progress;

            _ref.rootTransform.position = Vector3.Lerp(_startPos, _targetPos, easedProgress);

            if (_timer >= _duration)
            {
                _ref.rootTransform.position = _targetPos;
                _ref.animator.SetTrigger(FinishedMove);
                _active = false;
            }
        }
    }
}