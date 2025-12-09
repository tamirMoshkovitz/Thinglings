using UnityEngine;

namespace _SLIME.Gameplay.Slime.Scripts.new_scripts
{
    public class SlimeData
    {
        public bool Connected { get; set; }
        
        public float Distance { get; set; }
        
        public float MaxStretch { get; set; }
        
        public bool ReachedMaxStretch => Distance >= MaxStretch;
        
        public float StretchRatio => Mathf.Clamp01(Distance / MaxStretch);
    }
}