using System.Collections;
using _SLIME.BaseScripts;
using _SLIME.Core.Audio.FMOD.Scripts;
using FMODUnity;
using UnityEngine;
using UnityEngine.Serialization;

public class IcicleLogic : MonoBehaviour
{
    private static readonly int IcycleHit = Animator.StringToHash("IcycleHit");

    [Header("Visuals")]
    [SerializeField] private Animator icicleAnimator;
    [SerializeField] private RuntimeAnimatorController[] animatorVariants;
    [Tooltip("Particle GameObject (prefab) to spawn when the icicle is activated.")]
    [SerializeField] private GameObject particlePrefab;

    [Header("Fall")]
    [Tooltip("Delay (seconds) before the icicle starts falling after ActivateFall is called.")]
    [SerializeField] private float fallDelay = 0.5f;

    [Header("Audio")]
    [SerializeField] private EventReference icicleBreakSfx;

    private Rigidbody2D _rigidbody2D;
    private Collider2D _col;
    private Coroutine _activeCoroutine;
    private bool _hit;
    private bool _isFalling;
    private GameObject _spawnedParticle;

    private void Awake()
    {
        _rigidbody2D = GetComponent<Rigidbody2D>();
        _col = GetComponent<Collider2D>();
    }

    private void OnEnable()
    {
        ResetToHanging();
    }

   
    private void ResetToHanging()
    {
        if (_activeCoroutine != null)
        {
            StopCoroutine(_activeCoroutine);
            _activeCoroutine = null;
        }

        if (animatorVariants is { Length: > 0 })
        {
            icicleAnimator.runtimeAnimatorController = animatorVariants[Random.Range(0, animatorVariants.Length)];
        }

        _rigidbody2D.bodyType = RigidbodyType2D.Kinematic;
        _rigidbody2D.linearVelocity = Vector2.zero;
        _rigidbody2D.angularVelocity = 0f;
        
        _col.enabled = false;
        _hit = false;
        _isFalling = false;

        icicleAnimator.Rebind();
        icicleAnimator.Update(0f);
    }

    public void ActivateFall()
    {
        if (_activeCoroutine != null) StopCoroutine(_activeCoroutine);
        _spawnedParticle = Instantiate(particlePrefab, transform.position, Quaternion.identity, transform);
        _activeCoroutine = StartCoroutine(ActivateFallAfterDelay());
    }

    private IEnumerator ActivateFallAfterDelay()
    {
        yield return new WaitForSeconds(fallDelay);
        _activeCoroutine = null;
        _isFalling = true;
        _col.enabled = true;
        _rigidbody2D.bodyType = RigidbodyType2D.Dynamic;
        _rigidbody2D.gravityScale = 1.5f;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        ProcessHit(collision.gameObject);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        ProcessHit(collision.gameObject);
    }

    private void ProcessHit(GameObject otherGo)
    {
        if (_hit || !_isFalling) return;

        if (otherGo.TryGetComponent<IHealth>(out IHealth h))
        {
            _hit = true;
            h.TakeDamage();
            TriggerImpactVisuals();
            return;
        }

        if (!otherGo.CompareTag("Wall") && otherGo.layer != LayerMask.NameToLayer("Walls")) return;
        _hit = true;
        TriggerImpactVisuals();
    }

    private void TriggerImpactVisuals()
    {
        _rigidbody2D.linearVelocity = Vector2.zero;
        _rigidbody2D.bodyType = RigidbodyType2D.Static;
        
        icicleAnimator.SetTrigger(IcycleHit);
        SFXPlayer.Play(icicleBreakSfx);

        _activeCoroutine ??= StartCoroutine(WaitForDissolveAndDisable());
    }

    private IEnumerator WaitForDissolveAndDisable()
    {
        yield return new WaitForSeconds(0.05f); 
        
        float failSafeTimer = 2f;
        float elapsed = 0f;

        while (elapsed < failSafeTimer)
        {
            var state = icicleAnimator.GetCurrentAnimatorStateInfo(0);
            if (state.IsName("IcicleDissolve") && state.normalizedTime >= 1.0f)
            {
                break;
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        gameObject.SetActive(false); 
        _activeCoroutine = null;
    }
}