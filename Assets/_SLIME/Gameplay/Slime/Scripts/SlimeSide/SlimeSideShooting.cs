using System;
using System.Collections.Generic;
using _SLIME.Projectiles;
using _SLIME.Utils;
using DG.Tweening;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

namespace _SLIME.Slime
{
    [Serializable]
    public struct SlimeSideShootingSettings
    {
        public const float MinPossibleEnergy = 0f;
        public const float MaxPossibleEnergy = 1.0f;
        
        [BigHeader("GAMEPLAY LOGIC", HeaderColor.Blue,20)]
        
        [Header("Damage")]
        public float maxDamage;
        public float minDamage;
        [Tooltip("Controls how damage values interpolate")]
        public TweenDefinition DamageTween;
        
        
        [Header("Energy")]
        [Range(MinPossibleEnergy, MaxPossibleEnergy)]
        public float maxAddedEnergyPerFrame;
        [Range(MinPossibleEnergy, MaxPossibleEnergy)]
        public float minAddedEnergyPerFrame;
        
        [FormerlySerializedAs("EnergyTween")] [Tooltip("Controls how energy added values interpolate")]
        public TweenDefinition EnergyAddTween;
        
        [Range(MinPossibleEnergy, MaxPossibleEnergy)]
        public float maxLostEnergyPerShot;
        [Range(MinPossibleEnergy, MaxPossibleEnergy)]
        public float minLostEnergyPerShot;
        
        [Tooltip("Controls how energy added values interpolate")]
        public TweenDefinition EnergyLossTween;

        [Header("Speed")]
        public float maxSpeed;
        public float minSpeed;
        [Tooltip("Controls how speed values interpolate")]
        public TweenDefinition SpeedTween;
        
        [Header("TurnSmoothness")]
        public float maxTurnSmoothnes;
        public float minTurnSmoothnes;
        [Tooltip("Controls how turn Smothness values interpolate")]
        public TweenDefinition turnSmoothnesTween;

        
        [BigHeader("Visualization", HeaderColor.Red,20)]
        [Tooltip("Controls how the color changes over time")]
        public TweenDefinition ColorTween;
        [Header("Color")]
        public List<HSVColor> EnergyColors;
        
        [BigHeader("Position", HeaderColor.Cyan,20)]
        [Header("Buffer")]
        public float buffer;
    }
    
    public struct SlimeSideShootingReqComponents
    {
        public Renderer renderer;
        public Transform target;
        public BulletMonoPool pool;

        public SlimeSideShootingReqComponents(Renderer renderer, Transform target, BulletMonoPool pool)
        {
            this.renderer = renderer;
            this.target = target;
            this.pool = pool;
        }
    }
    
    
    public class SlimeSideShooting
    {
        private readonly SlimeSide _slimeSide;
        private readonly SlimeSideShootingSettings _shootingSetting;
        private readonly SlimeSideShootingReqComponents _shootingReqComponents;

        private float _currentEnergy;

        public SlimeSideShooting(SlimeSide slimeSide, SlimeSideShootingSettings shootingSettings,
            SlimeSideShootingReqComponents shootingReqComponents)
        {
            _slimeSide = slimeSide;
            _shootingSetting = shootingSettings;
            _shootingReqComponents = shootingReqComponents;
            _currentEnergy = (SlimeSideShootingSettings.MaxPossibleEnergy 
                              + SlimeSideShootingSettings.MinPossibleEnergy) / 2;
            if (_shootingSetting.minAddedEnergyPerFrame > _shootingSetting.maxAddedEnergyPerFrame)
            {
                _shootingSetting.maxAddedEnergyPerFrame = _shootingSetting.minAddedEnergyPerFrame;
                Debug.LogWarning("maxAddedEnergyPerFrame was lower than minAddedEnergyPerFrame");
            }
        }

        public void Update()
        {
            float addedEnergy = DOVirtual.EasedValue(_shootingSetting.minAddedEnergyPerFrame, 
                _shootingSetting.maxAddedEnergyPerFrame, _currentEnergy, _shootingSetting.EnergyAddTween.easeType );
            _currentEnergy = Mathf.Min(SlimeSideShootingSettings.MaxPossibleEnergy, 
                _currentEnergy + addedEnergy * Time.deltaTime); 
            float easedT = DOVirtual.EasedValue(
                0f, 1f, _currentEnergy, _shootingSetting.ColorTween.easeType);
            HSVColor color = HSVColor.LerpMulti(easedT, _shootingSetting.EnergyColors.ToArray());
            _shootingReqComponents.renderer.material.color = color.ToRGB();
            
            Debug.Log(_currentEnergy);
        }

        public void OnShoot(InputAction.CallbackContext context)
        {
            float lostEenrgy = DOVirtual.EasedValue(_shootingSetting.minLostEnergyPerShot,_shootingSetting.maxLostEnergyPerShot,
                _currentEnergy,_shootingSetting.EnergyLossTween.easeType);
            if(lostEenrgy > _currentEnergy) return;
            _currentEnergy -= lostEenrgy;
            var bullet = _shootingReqComponents.pool.Get();
            float damage = DOVirtual.EasedValue(_shootingSetting.minDamage, 
                _shootingSetting.maxDamage, _currentEnergy, _shootingSetting.DamageTween.easeType );
            float speed = DOVirtual.EasedValue(_shootingSetting.minSpeed, 
                _shootingSetting.maxSpeed, _currentEnergy, _shootingSetting.SpeedTween.easeType );
            float turnSmoothness = DOVirtual.EasedValue(_shootingSetting.minTurnSmoothnes, 
                _shootingSetting.maxTurnSmoothnes, _currentEnergy, _shootingSetting.turnSmoothnesTween.easeType );
            bullet.Activate(new BulletInitData(
                _shootingReqComponents.target,
                _slimeSide.Position,
                speed,
                _shootingSetting.buffer,
                _shootingReqComponents.pool,
                turnSmoothness,
                damage
                ));
            
            
        }
    }
}