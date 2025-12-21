namespace _SLIME.Utils
{
    using UnityEngine;
    using DG.Tweening;

    [System.Serializable]
    public struct TweenDefinition
    {
        public Ease easeType;      
        public AnimationCurve curve; 
    }
}