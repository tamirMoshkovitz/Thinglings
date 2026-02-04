using System.Collections;
using _SLIME.BaseScripts;
using _SLIME.GameLoop;
using UnityEngine;

namespace _SLIME.Scenes.Ending.Scripts
{
    [RequireComponent(typeof(Animator), typeof(Collider2D))]
    public class EndingScreenManager: ProjectMonoBehavior
    {
        [SerializeField] private GameObject gradientBackground;
        [SerializeField] private Transform slimeEndingPosition;
        [SerializeField] private Transform cameraEndingPosition;
        [SerializeField] private GameObject firstSlime;
        [SerializeField] private GameObject secondSlime;
        [SerializeField] private Camera camera;
        [SerializeField] private float phaseDelay = 1f;
        [SerializeField] private float slimeRiseDuration = 4f;
        [SerializeField] private float subtitlesDuration = 15f;
        
        private static readonly int StartEnding = Animator.StringToHash("start end");
        private static readonly int EndComplete = Animator.StringToHash("end complete");
        private Animator _animator;
        private Collider2D _collider;
        private bool _enteredGradientTrigger;

        private void Awake()
        {
            _animator = GetComponent<Animator>();
            _collider = GetComponent<Collider2D>();
            
            if (_collider.enabled) // Debug scene
                OnSlimeWon();
        }

        private void OnEnable()
        {
            GameEvents.SlimeWon += OnSlimeWon;
        }

        private void OnDisable()
        {
            GameEvents.SlimeWon -= OnSlimeWon;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
                _enteredGradientTrigger = true;
        }
        
        private IEnumerator EndingCoroutine()
        {
            yield return WaitForSlimesToRise();
            yield return RiseSlimes();
            yield return new WaitForSeconds(phaseDelay);
            yield return ShowText();
            //TODO: load starting scene
        }
        
        private IEnumerator WaitForSlimesToRise()
        {
            gradientBackground.SetActive(true);
            _collider.enabled = true;
            _animator.SetTrigger(StartEnding);

            while (!_enteredGradientTrigger)
            {
                yield return null;
            }
        }

        private IEnumerator RiseSlimes()
        {
            Vector3 firstSlimeStartPos = firstSlime.transform.position;
            Vector3 secondSlimeStartPos = secondSlime.transform.position;
            Vector3 cameraStartPos = camera.transform.position;
            
            Vector3 firstSlimeEndPos = new Vector3(firstSlimeStartPos.x, slimeEndingPosition.position.y, firstSlimeStartPos.z);
            Vector3 secondSlimeEndPos = new Vector3(secondSlimeStartPos.x, slimeEndingPosition.position.y, secondSlimeStartPos.z);
            Vector3 cameraEndPos = new Vector3(cameraStartPos.x, cameraEndingPosition.position.y, cameraStartPos.z);
            
            float elapsedTime = 0f;
            while (elapsedTime < slimeRiseDuration)
            {
                float t = elapsedTime / slimeRiseDuration;
                
                firstSlime.transform.position = Vector3.Lerp(firstSlimeStartPos, firstSlimeEndPos, t);
                secondSlime.transform.position = Vector3.Lerp(secondSlimeStartPos, secondSlimeEndPos, t);
                camera.transform.position = Vector3.Lerp(cameraStartPos, cameraEndPos, t);
                
                FMOD.RESULT result = FMODUnity.RuntimeManager.StudioSystem.setParameterByName("light", t);
                if (result != FMOD.RESULT.OK)
                {
                    Debug.LogError($"Failed to set FMOD parameter 'light': {result}");
                }
                
                elapsedTime += Time.deltaTime;
                yield return null;
            }
        }
        
        private IEnumerator ShowText()
        {
            _animator.SetTrigger(EndComplete);
            yield return new WaitForSeconds(subtitlesDuration);
        }

        private void OnSlimeWon()
        {
            StartCoroutine(EndingCoroutine());
            GameEvents.FmodPhaseSix?.Invoke();
        }
    }
}