using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;

public class PeekingCreature : MonoBehaviour
{
    [Header("Relative Offsets")]
    public Vector3 startOffset = Vector3.zero; 
    public Vector3 peekOffset = new Vector3(0, 1, 0);

    [Header("Movement Settings")]
    public float moveDuration = 0.8f;
    public float peekStayTime = 2.0f;

    private SpriteRenderer _myRenderer;
    private SortingGroup _parentGroup;
    private SpriteRenderer _parentRenderer;
    
    private Vector3 _localStartPoint;
    private Vector3 _localEndPoint;
    private bool _isBusy = false;

    public bool IsBusy => _isBusy;

    void Awake()
    {
        _myRenderer = GetComponent<SpriteRenderer>();
        
        _parentGroup = GetComponentInParent<SortingGroup>();
        if (_parentGroup == null)
        {
            _parentRenderer = transform.parent?.GetComponent<SpriteRenderer>();
        }
        _localStartPoint = transform.localPosition + (transform.localRotation * startOffset);
        _localEndPoint = transform.localPosition + (transform.localRotation * peekOffset);

        transform.localPosition = _localStartPoint;
        _myRenderer.enabled = false; 
    }

    void LateUpdate()
    {
        if (_parentGroup)
        {
            _myRenderer.sortingLayerID = _parentGroup.sortingLayerID;
            _myRenderer.sortingOrder = _parentGroup.sortingOrder - 1; 
        }
        else if (_parentRenderer)
        {
            _myRenderer.sortingLayerID = _parentRenderer.sortingLayerID;
            _myRenderer.sortingOrder = _parentRenderer.sortingOrder - 1;
        }
    }

    public void Peek()
    {
        if (_isBusy || !gameObject.activeInHierarchy) return;
        
        _myRenderer.enabled = true;
        StartCoroutine(PeekRoutine());
    }

    private IEnumerator PeekRoutine()
    {
        _isBusy = true;

        yield return StartCoroutine(MoveSmooth(_localStartPoint, _localEndPoint, moveDuration));
        
        yield return new WaitForSeconds(peekStayTime);

        yield return StartCoroutine(MoveSmooth(_localEndPoint, _localStartPoint, moveDuration));

        _isBusy = false;
        _myRenderer.enabled = false;
    }

    private IEnumerator MoveSmooth(Vector3 from, Vector3 to, float duration)
    {
        float elapsed = 0;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            
            t = t * t * (3f - 2f * t); 
            
            transform.localPosition = Vector3.Lerp(from, to, t);
            yield return null;
        }
        transform.localPosition = to;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.matrix = transform.parent != null ? transform.parent.localToWorldMatrix : Matrix4x4.identity;

        Vector3 startPos = transform.localPosition + (transform.localRotation * startOffset);
        Vector3 endPos = transform.localPosition + (transform.localRotation * peekOffset);
        
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(startPos, 0.1f);
        
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(endPos, 0.1f);
        
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(startPos, endPos);
    }
}