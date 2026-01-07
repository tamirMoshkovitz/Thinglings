using System;
using _SLIME.BaseScripts;
using UnityEngine;
[CreateAssetMenu(fileName = "ArtConfig", menuName = "ArtConfigurations")]
public class ArtConfigurations : TabbedScriptableObject
{
    [Serializable]
    public class ParallaxSettings 
    {
        [Header("Depth Configuration")]
        [Tooltip("Defines how sensitivity changes with Z depth.\n" +
                 "X-Axis = Z Position\n" +
                 "Y-Axis = Sensitivity (1 = Moves with player, 0 = Static background)")]
        public AnimationCurve depthSensitivityCurve = new AnimationCurve(
            new Keyframe(0, 1.0f),
            new Keyframe(20, 0.5f),
            new Keyframe(50, 0.0f)
        );

        [Header("Axis Multipliers")]
        [Tooltip("Global multiplier for Horizontal movement.")]
        public float sensitivityMultiplierX = 1.0f;
        
        [Tooltip("Global multiplier for Vertical movement (Usually keep this lower than X).")]
        public float sensitivityMultiplierY = 0.1f;

        [Header("Clamping Limits")]
        [Tooltip("Max distance objects can shift on X from their origin.")]
        public float maxShiftX = 10.0f;
        [Tooltip("Max distance objects can shift on Y from their origin.")]
        public float maxShiftY = 10f;
    
        [Header("General")]
        [Tooltip("Smoothing factor for movement, higher is smoother.")]
        [Range(0f, 1f)]
        public float smoothing = 0.1f;
    }
    
    [Serializable]
    public class TunnelMovementSettings
    {
        [Header("Movement Control")]
        [Tooltip("The speed at which the tunnel moves towards the player. Fits perspective and distribution effects.")]
        public float movementSpeed;
        [Tooltip("The Z position at which objects respawn at the start of the tunnel.")]
        public float respawnPositionZ;
        [Tooltip("The Z position at which objects despawn at the end of the tunnel.")]
        public float despawnPositionZ = 60f;

        [Header("Scale Settings")]
        [Tooltip("Adjusts the cameras focal length effect on the tunnel. Higher = more pronounced 3D effect.")]
        public float cameraFocalScale = 20f;
        public float baseLayerScale = 1f;
        
        [Header("Brightness Settings")]
        [Tooltip("If checked, objects further in depth get darker.")]
        public bool useDepthDarkening = true;
        [Tooltip("The curve that controls the brightness behavior based on depth.")]
        public AnimationCurve brightnessCurve = new AnimationCurve(new Keyframe(0, 1), new Keyframe(1, 0.5f));

        [Header("Opacity Controls")]
        public AnimationCurve opacityCurve;
        [Tooltip("If checked, objects fade in at the start. If not, they pop in fully visible.")]
        public bool fadeInAtEntrance = true;
        [Tooltip("If checked, objects fade out at the end. If not, they pop out fully visible.")]
        public bool fadeOutAtExit = true;

        [Header("Automation Settings (No need to tweak manually but available - advanced)")]
        [Tooltip("If checked, speed controls the 3D effect.")]
        public bool linkPerspectiveToSpeed = true;
        [Tooltip("The speed at which the perspective effect reaches its target.")]
        public float maxEffectSpeed = 20f;
            
        [Tooltip("The perspective value (0-1) reached when moving at Max Speed. 1 = 3D, 0 = 2D ")]
        [Range(0f, 1f)] public float targetPerspectiveAtMaxSpeed = 1f;
        
    }
    
    #region Tunnel Setup
    
    [Tab("Tunnel")]
    [Header("Tunnel Movement Settings")]
    public TunnelMovementSettings tunnelMovementSettings = new TunnelMovementSettings();

    #endregion

    #region Parallax Setup
    [Tab("Parallax")]
    [Header("Global Parallax Settings")]
    public ParallaxSettings parallaxSettings = new ParallaxSettings();
    #endregion
    
    
}