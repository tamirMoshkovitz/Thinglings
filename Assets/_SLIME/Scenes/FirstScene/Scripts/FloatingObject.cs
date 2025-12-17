using System;
using System.Collections;
using UnityEngine;
using DG.Tweening;
using UnityEngine.Serialization;

public class FloatingObject : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform objectTransform;

    [Header("Entry Settings")]
    [SerializeField] private Vector3 targetPosition;
    [SerializeField] private float moveDuration = 2.0f;
    [SerializeField] private Ease entryEase = Ease.InOutQuad;

    [Header("Floating Settings")]
    [SerializeField] private float height = 0.5f; 
    [SerializeField] private float speed = 2f;

    private Tween _floatTween;

    public IEnumerator Activate()
    {
        Tween moveTween = transform.DOMove(targetPosition, moveDuration).SetEase(entryEase);
        yield return moveTween.WaitForCompletion();
        
        float floatDuration = Mathf.PI / speed;
        float targetY = gameObject.transform.position.y + height;

        _floatTween = objectTransform.DOLocalMoveY(targetY, floatDuration)
            .SetEase(Ease.InOutSine);
        
        yield return _floatTween.WaitForCompletion();
        
        _floatTween = objectTransform.DOLocalMoveY(targetY, floatDuration)
            .SetEase(Ease.InOutSine)
            .SetLoops(-1, LoopType.Yoyo);
    }


    private void OnDisable()
    {
        _floatTween?.Kill();
        transform.DOKill();
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(targetPosition, 0.5f);
    }
}