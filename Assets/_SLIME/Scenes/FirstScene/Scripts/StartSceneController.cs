using System;
using System.Collections;
using _SLIME.BaseScripts;
using _SLIME.Slime;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;


namespace _SLIME.StartScene
{


    public class StartSceneController : ProjectMonoBehavior
    {
        [Header("Spawned Objects Settings")] [SerializeField]
        private float timeBetweenSpawns = 5f;

        [Header("Camera Settings")] [SerializeField]
        private Camera mainCamera;

        [SerializeField] private float zoomOutAmount = 1.0f;
        [SerializeField] private Ease zoomEase = Ease.OutQuad;
        [SerializeField] private float zoomSpeed = 5f;

        [Header("Bloom Settings")] [SerializeField]
        private VolumeProfile bloomVolume;

        [SerializeField] private float bloomFinalIntensity = 100f;
        [SerializeField] private float bloomBaseIntensity;




        [SerializeField] private GameObject transition;

        [Header("Spawn Objects")] [SerializeField]
        private GameObject Rock;

        [SerializeField] private Animator rockAnimator;
        [SerializeField] private FloatingObject[] floatingObjects;
        [SerializeField] private GameObject Slime;

        private Bloom _bloom;
        private bool _ready;

        private void OnEnable()
        {
            SlimeEvents.SlimeTears += ReadyForSceneChange;
        }

        private void ReadyForSceneChange()
        {
            _ready = true;
        }

        private void OnDisable()
        {
            SlimeEvents.SlimeTears -= ReadyForSceneChange;
        }

        private void Start()
        {

            foreach (Transform child in transform)
            {
                child.gameObject.SetActive(false);
            }

            if (bloomVolume.TryGet(out Bloom bloomComponent))
            {
                _bloom = bloomComponent;
            }

            StartCoroutine(BeginningSceneRoutine());
        }

        #region Sequential CoroutineStates

        private IEnumerator BeginningSceneRoutine()
        {
            yield return new WaitForSeconds(timeBetweenSpawns);
            StartCoroutine(FirstObjectEnteringRoutine());
        }



        private IEnumerator FirstObjectEnteringRoutine()
        {
            yield return SpawnObjectRoutine(0, "TriggerPhase2", "Phase2");
            StartCoroutine(SecondObjectEnteringRoutine());
        }

        private IEnumerator SecondObjectEnteringRoutine()
        {
            yield return SpawnObjectRoutine(1, "TriggerPhase3", "Phase3");
            StartCoroutine(ThirdObjectEnteringRoutine());
        }

        private IEnumerator ThirdObjectEnteringRoutine()
        {
            yield return SpawnObjectRoutine(2, "TriggerPhase4", "Phase4");
            StartCoroutine(FourthObjectEnteringRoutine());
        }

        private IEnumerator FourthObjectEnteringRoutine()
        {
            floatingObjects[3].gameObject.SetActive(true);
            CameraShift(3);
            yield return floatingObjects[3].Activate();
            yield return TriggerAndWaitForState(rockAnimator, "TriggerSlime", "Phase4ToSlime");


            StartCoroutine(SlimeTearsRoutine());
        }

        private IEnumerator SlimeTearsRoutine()
        {
            Rock.SetActive(false);
            Slime.SetActive(true);
            while (!_ready) yield return null;
            transition.SetActive(true);
        }

        #endregion




        /// <summary>
        /// Generic routine for spawning a floating object with animation trigger
        /// </summary>
        private IEnumerator SpawnObjectRoutine(int objectIndex, string triggerName, string stateName)
        {
            floatingObjects[objectIndex].gameObject.SetActive(true);
            CameraShift(objectIndex);

            yield return floatingObjects[objectIndex].Activate();
            yield return TriggerAndWaitForState(rockAnimator, triggerName, stateName);
            yield return new WaitForSeconds(timeBetweenSpawns);
        }

        private void CameraShift(int objectIndex)
        {
            float targetSize = mainCamera.orthographicSize + zoomOutAmount;
            mainCamera.DOOrthoSize(targetSize, zoomSpeed).SetEase(zoomEase);

            float progress = (float)objectIndex / floatingObjects.Length;
            float newBaseIntensity = Mathf.Lerp(bloomBaseIntensity, bloomFinalIntensity, progress);
        }

        #region Animator Utilities

        /// <summary>
        /// Triggers an animation and waits until it completes
        /// </summary>
        private IEnumerator TriggerAndWaitForState(Animator animator, string triggerName, string targetStateName,
            int layer = 0)
        {
            // Trigger the animation
            animator.SetTrigger(triggerName);

            // Wait until we enter the target state
            yield return new WaitUntil(() => animator.GetCurrentAnimatorStateInfo(layer).IsName(targetStateName));

            // Wait until the animation completes (normalizedTime >= 1)
            yield return new WaitUntil(() =>
                animator.GetCurrentAnimatorStateInfo(layer).IsName(targetStateName) &&
                animator.GetCurrentAnimatorStateInfo(layer).normalizedTime >= 1.0f);
        }

        #endregion

    }
}