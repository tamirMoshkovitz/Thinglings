using System.Collections;
using _SLIME.BaseScripts;
using DG.Tweening;
using UnityEngine;

namespace _SLIME.Envierment.Earthquake.Scriptables
{
    [CreateAssetMenu(fileName = "EarthquakeConfig", menuName = "Scriptable Objects/EarthQuakeConfiguration")]
    public class EarthquakeUtil: TabbedScriptableObject
    {
        [SerializeField] private float duration;
        [SerializeField] private Vector3 strength;
        [SerializeField] private int vibrato;
        [SerializeField] private float randomness;
        [SerializeField] private bool snapping;
        [SerializeField] private bool fadeOut;
        [SerializeField] private Ease ease;
        [SerializeField] private float animationDelay;
        
        public float Duration { get => duration; }

        public IEnumerator EarthquakeCoroutine(Camera camera, Animator iciclesAnimator, int triggerHash)
        {
            float timer = 0f;
            
            DOShakeWithScriptableConfig(camera.transform);

            while (timer < animationDelay)
            {
                yield return null;
                timer += Time.deltaTime;
            }
            
            iciclesAnimator?.SetTrigger(triggerHash);
        }

        private Tweener DOShakeWithScriptableConfig(Transform target)
        {
            return target.DOShakePosition(
                duration,
                strength,
                vibrato,
                randomness,
                snapping,
                fadeOut
            ).SetEase(ease);
        }
    }
}