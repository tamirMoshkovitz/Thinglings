using System;
using _SLIME.BaseScripts;
using UnityEngine;
using UnityEngine.Serialization; // Required to save your old data

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

    [Tab("Parallax")]
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