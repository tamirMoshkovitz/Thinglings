using System;
using _SLIME.BaseScripts;
using _SLIME.GameLoop;
using UnityEngine;

namespace _SLIME.Gameplay.Enemies.Boss.Pinball_Boss.Scripts
{
    public class PinballBossAnimations: ProjectMonoBehavior
    {
        private static readonly int Property = Animator.StringToHash("Icicle Break Counter");
        private Animator _animator;
        private int _icicleBreakCounter;

        private void Awake()
        {
            _animator = GetComponent<Animator>();
        }

        private void OnEnable()
        {
            GameEvents.IcicleCrumbled += OnIcicleCrumble;
        }

        private void OnDisable()
        {
            GameEvents.IcicleCrumbled -= OnIcicleCrumble;
        }
        
        private void OnIcicleCrumble()
        {
            _animator.SetInteger(Property, ++_icicleBreakCounter);
        }
    }
}