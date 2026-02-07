using System.Collections;
using _SLIME.BaseScripts;
using _SLIME.Core.Audio.FMOD.Scripts;
using FMODUnity;
using UnityEngine;

public class IcicleLogic : MonoBehaviour
{
    [Header("Visuals")]
    [SerializeField] private Animator icicleAnimator;
    [SerializeField] private RuntimeAnimatorController[] animatorVariants; 

    [Header("Audio")]
    [SerializeField] private EventReference IcicleBreakSFX;

    private Rigidbody2D _rigidbody2D;
    private Collider2D _col;
    private Coroutine _activeCoroutine;
    private bool _hit;
    private float _activateFallTime = -1f;

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

        // Pick one of the 3 Animators randomly
        if (animatorVariants != null && animatorVariants.Length > 0)
        {
            icicleAnimator.runtimeAnimatorController = animatorVariants[Random.Range(0, animatorVariants.Length)];
        }

        _rigidbody2D.bodyType = RigidbodyType2D.Kinematic;
        _rigidbody2D.linearVelocity = Vector2.zero;
        _rigidbody2D.angularVelocity = 0f;
        _col.enabled = false;
        _hit = false;
        _activateFallTime = -1f;

        icicleAnimator.Rebind();
        icicleAnimator.Update(0f);
    }

    public void ActivateFall()
    {
        _activateFallTime = Time.time;
        _col.enabled = true;
        _rigidbody2D.bodyType = RigidbodyType2D.Dynamic;
        _rigidbody2D.gravityScale = 1f;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (_hit || _activateFallTime < 0f || Time.time - _activateFallTime < 1f) return;
        _hit = true;
        if (collision.gameObject.CompareTag("Wall"))
        {
            StartCoroutine(WaitAndSetFalse());
            return;
        };
        HandleImpact(collision.attachedRigidbody);
        
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (_hit || _activateFallTime < 0f || Time.time - _activateFallTime < 1f) return;
        _hit = true;
        if (collision.gameObject.CompareTag("Wall"))
        {
            StartCoroutine(WaitAndSetFalse());
            return;
        };
        HandleImpact(collision.rigidbody);
    }

    private IEnumerator WaitAndSetFalse()
    {
        yield return new WaitForSeconds(3f);
        gameObject.SetActive(false); 
    }


    private void HandleImpact(Rigidbody2D otherBody)
    {
        
        if (otherBody && otherBody.TryGetComponent<IHealth>(out IHealth h))
        {
            h.TakeDamage();
        }

        icicleAnimator.SetTrigger("IcycleHit");
        SFXPlayer.Play(IcicleBreakSFX);

        if (_activeCoroutine == null)
        {
            _activeCoroutine = StartCoroutine(WaitForDissolveAndDisable());
        }
    }
    
    private IEnumerator WaitForDissolveAndDisable()
    {
        while (!icicleAnimator.GetCurrentAnimatorStateInfo(0).IsName("IcicleDissolve"))
            yield return null;
        
        while (icicleAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1.0f)
            yield return null;

        gameObject.SetActive(false); 
    }
}