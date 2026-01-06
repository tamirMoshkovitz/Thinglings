using System;
using _SLIME.BaseScripts;
using UnityEngine;
[CreateAssetMenu(fileName = "ArtConfig", menuName = "ArtConfigurations")]
public class ArtConfigurations : TabbedScriptableObject
{
    [Serializable]
    public class ParallaxSettings 
    {
        [Header("Horizontal (X)")]
        [Tooltip("Max distance X can move.")]
        public float maxShiftX = 5.0f;

        [Tooltip("1.0 = Moves with player")]
        [Range(-1.0f, 1.0f)]
        public float sensitivityX = 0.5f;

        [Header("Vertical (Y)")]
        [Tooltip("Max distance Y can move. Keeps smaller than X")]
        public float maxShiftY = 0.5f;

        [Tooltip("Vertical sensitivity. 1.0 = Moves with player, keep lower")]
        [Range(-0.5f, 0.5f)]
        public float sensitivityY = 0.1f;
    
        [Header("General")]
        [Tooltip("Smoothing factor for movement, higher is smoother.")]
        [Range(0f, 0.5f)]
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
    
    #region Parallax Setup
    [Tab("Parallax")]
    [Header("Parallax Layers Settings")]
    [Header("Layer 1 (Front)")]
    [SerializeField] private ParallaxSettings parallaxLayer1 = new ParallaxSettings();

    [Tab("Parallax")]
    [Header("Layer 2")]
    [SerializeField] private ParallaxSettings parallaxLayer2 = new ParallaxSettings();

    [Tab("Parallax")]
    [Header("Layer 3")]
    [SerializeField] private ParallaxSettings parallaxLayer3 = new ParallaxSettings();

    [Tab("Parallax")]
    [Header("Layer 4")]
    [SerializeField] private ParallaxSettings parallaxLayer4 = new ParallaxSettings();

    [Tab("Parallax")]
    [Header("Layer 5 (Back)")]
    [SerializeField] private ParallaxSettings parallaxLayer5 = new ParallaxSettings();
    
    [Tab("Parallax")]
    [Header("Boss Layer")]
    [SerializeField] private ParallaxSettings parallaxBossLayer = new ParallaxSettings();
    #endregion

    #region Tunnel Setup
    
    [Tab("Tunnel")]
    [Header("Tunnel Movement Settings")]
    public TunnelMovementSettings tunnelMovementSettings = new TunnelMovementSettings();

    #endregion
    
    public ParallaxSettings GetSettings(ParallaxLayers layer)
    {
        switch (layer)
        {
            case ParallaxLayers.Layer1: return parallaxLayer1;
            case ParallaxLayers.Layer2: return parallaxLayer2;
            case ParallaxLayers.Layer3: return parallaxLayer3;
            case ParallaxLayers.Layer4: return parallaxLayer4;
            case ParallaxLayers.Layer5: return parallaxLayer5;
            case ParallaxLayers.Boss: return parallaxBossLayer;
            default: return parallaxLayer1;
        }
    }
}