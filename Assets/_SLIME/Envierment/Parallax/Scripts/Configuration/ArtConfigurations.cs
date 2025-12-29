using System;
using _SLIME.BaseScripts;
using UnityEngine;

[CreateAssetMenu(fileName = "ArtConfig", menuName = "ArtConfigurations")]
public class ArtConfigurations : TabbedScriptableObject
{
    // CHANGE 1: Change 'struct' to 'class'
    [Serializable]
    public class ParallaxSettings 
    {
        [Tooltip("The absolute maximum distance this object can move from its start position.")]
        public float maxShift = 2.0f;

        [Tooltip("How sensitive the movement is. Lower = Slower reaction.")]
        [Range(0, 0.5f)]
        public float sensitivity = 0.05f;
    
        [Tooltip("Smoothing factor for the movement. Higher = Smoother.")]
        [Range(0f, 1f)]
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
    
    public ParallaxSettings GetSettings(ParallaxLayers layer)
    {
        switch (layer)
        {
            case ParallaxLayers.Layer1: return parallaxLayer1;
            case ParallaxLayers.Layer2: return parallaxLayer2;
            case ParallaxLayers.Layer3: return parallaxLayer3;
            case ParallaxLayers.Layer4: return parallaxLayer4;
            case ParallaxLayers.Layer5: return parallaxLayer5;
            default: return parallaxLayer1;
        }
    }
}