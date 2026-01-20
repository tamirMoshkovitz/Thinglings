using System.Collections;
using UnityEngine;

public class AttackVisualWarning : MonoBehaviour
{
    [Header("Pulse Settings")]
    [SerializeField] private float pulseSpeed = 4f;
    [Range(0, 1)] 
    [SerializeField] private float maxAlpha = 0.7f;

    private SpriteRenderer _spriteRenderer;
    private Color _baseColor;

    private void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _baseColor = _spriteRenderer.color;
    }

    private void OnEnable()
    {
        Color c = _baseColor;
        c.a = 0f;
        _spriteRenderer.color = c;
            
        StartCoroutine(PulseRoutine());
    }

    private void OnDisable()
    {
        StopAllCoroutines();
    }

    private IEnumerator PulseRoutine()
    {
        float timer = 0f;
            
        while (true)
        {
            timer += Time.deltaTime;
            float alpha = Mathf.PingPong(timer * pulseSpeed, maxAlpha);

            Color c = _baseColor;
            c.a = alpha;
            _spriteRenderer.color = c;

            yield return null;
        }
    }
}
