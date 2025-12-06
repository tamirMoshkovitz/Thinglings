using UnityEngine;
using DG.Tweening; 
using System.Collections;
using UnityEngine.Serialization;

public class HandController : MonoBehaviour
{
    [Header("References")]
    public Transform player;        

    [Header("Position Settings")]
    public float hoverY = 20f;     
    public float prepareY = 18f;  
    public float floorY = -1.5f;    
    
    [Header("Attack Settings")]
    public float entryDuration = 0.5f; 
    public float trackDuration = 2.0f; 
    public float trackSpeed = 10f;     
    public float dropDuration = 0.2f;  
    public float riseDuration = 1.0f;
    
    [Header("Camera Shake")]
    [SerializeField] private Camera mainCamera;
    [SerializeField] private float shakeDuration = .8f;
    [SerializeField] private Vector3 shakeStrength = Vector3.up;
    [SerializeField] private int shakeVibrato = 10;
    [SerializeField] private float shakeRandomness = 90f;
    [SerializeField] private bool shakeFadeOut = true;
    [SerializeField] private ShakeRandomnessMode shakeRandomnessMode = ShakeRandomnessMode.Harmonic;
    
    [SerializeField] private bool isRightHand = false;
    [SerializeField] private Transform boundTransform; 
    

    private bool _isAttacking;

    public void StartAttack()
    {
        if(!_isAttacking) StartCoroutine(SmashRoutine());
    }

    IEnumerator SmashRoutine()
    {
        _isAttacking = true;
        yield return transform.DOMoveY(prepareY, entryDuration).SetEase(Ease.OutBack).WaitForCompletion();

        float timer = 0f;
        while (timer < trackDuration)
        {
            
            float currentX = transform.position.x;
            float targetX = isRightHand ? Mathf.Max(player.position.x, boundTransform.position.x) : Mathf.Min(player.position.x, boundTransform.position.x);
            float newX = Mathf.Lerp(currentX, targetX, Time.deltaTime * trackSpeed);

            transform.position = new Vector3(newX, prepareY, transform.position.z);
            
            timer += Time.deltaTime;
            yield return null;
        }
        

        yield return transform.DOMoveY(floorY, dropDuration).SetEase(Ease.InExpo).WaitForCompletion();
        
        mainCamera.DOShakePosition(shakeDuration, shakeStrength.normalized, shakeVibrato, shakeRandomness, shakeFadeOut, shakeRandomnessMode);
        yield return new WaitForSeconds(1.0f);
        

        yield return transform.DOMoveY(hoverY, riseDuration).SetEase(Ease.InOutSine).WaitForCompletion();

        _isAttacking = false;
    }
}