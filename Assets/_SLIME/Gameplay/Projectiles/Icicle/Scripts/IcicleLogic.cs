using System;
using UnityEngine;
using UnityEngine.Serialization;

public class IcicleLogic : MonoBehaviour
{
    [SerializeField] private Animator icicleAnimator;
    [SerializeField] private Transform spawnPoint;
    private new Rigidbody2D _rigidbody2D;
    private Collider2D _col;
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
    }

    public void ActivateFall()
    {
        _col.enabled = true;
        _rigidbody2D.bodyType = RigidbodyType2D.Dynamic;
        _rigidbody2D.gravityScale = 1f;
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        // Ask elad how to add the logic to the tear/ hit.
        gameObject.SetActive(false);
    }
}