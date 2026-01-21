using System;
using System.Collections;
using _SLIME.BaseScripts;
using _SLIME.Core.Audio.FMOD.Scripts;
using FMODUnity;
using UnityEngine;
using UnityEngine.Serialization;

public class IcicleLogic : MonoBehaviour
{
    [SerializeField] private Animator icicleAnimator;
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private TurnOffGameobjectAfterTime turnOffGameobjectAfterTime;
    [SerializeField] private EventReference IcicleBreakSFX;
    private new Rigidbody2D _rigidbody2D;
    private Collider2D _col;
    private Coroutine _courtine;
    private bool _hit;

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
        _rigidbody2D.bodyType = RigidbodyType2D.Kinematic;
        _rigidbody2D.linearVelocity = Vector2.zero;
        _rigidbody2D.angularVelocity = 0f;
        _col.enabled = false;
        gameObject.transform.position = spawnPoint.position;
        _hit = false;
        _courtine = null;
    }

    public void ActivateFall()
    {
        _col.enabled = true;
        _rigidbody2D.bodyType = RigidbodyType2D.Dynamic;
        _rigidbody2D.gravityScale = 1f;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(_hit) return;
        _hit = true;
        var rig = collision.attachedRigidbody;
        if (rig && rig.TryGetComponent<IHealth>(out IHealth h))
        {
            h.TakeDamage();
        }
        icicleAnimator.SetTrigger("IcycleHit");
        SFXPlayer.Play(IcicleBreakSFX);
        if(_courtine != null) return;
        _courtine = StartCoroutine(WaitForDissolveState());
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if(_hit) return;
        _hit = true;
        icicleAnimator.SetTrigger("IcycleHit");
        SFXPlayer.Play(IcicleBreakSFX);
        if(_courtine != null) return;
        _courtine = StartCoroutine(WaitForDissolveState());
    }
    
    private IEnumerator WaitForDissolveState()
    {
        // Wait until we enter the IcicleDissolve state
        while (!icicleAnimator.GetCurrentAnimatorStateInfo(0).IsName("IcicleDissolve"))
        {
            yield return null;
        }
        
        // Wait until the animation finishes
        while (icicleAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1.0f)
        {
            yield return null;
        }
        turnOffGameobjectAfterTime.TurnOff();
    }
}