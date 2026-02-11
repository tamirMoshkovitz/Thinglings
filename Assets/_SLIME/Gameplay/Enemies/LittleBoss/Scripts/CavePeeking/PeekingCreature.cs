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

    // These are set by the Manager now, but visible in Inspector for debug
    [Header("Current State (Read Only)")]
    public Color visibleColor = Color.white; 
    public Color hiddenColor = Color.black;

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
        
        // Cache parent references for sorting
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
        // Sync sorting order with parent every frame
        int order = GetTargetSortingOrder();
        _myRenderer.sortingLayerID = GetTargetSortingLayerID();
        _myRenderer.sortingOrder = order;
    }
    
    // Helper so Manager can ask "What layer are you on?"
    public int GetTargetSortingOrder()
    {
        if (_parentGroup) return _parentGroup.sortingOrder - 1;
        if (_parentRenderer) return _parentRenderer.sortingOrder - 1;
        
        // Fallback if no parent renderer found, check own component or default
        if (!_myRenderer) _myRenderer = GetComponent<SpriteRenderer>();
        return _myRenderer ? _myRenderer.sortingOrder : 0;
    }

    private int GetTargetSortingLayerID()
    {
        if (_parentGroup) return _parentGroup.sortingLayerID;
        if (_parentRenderer) return _parentRenderer.sortingLayerID;
        return _myRenderer ? _myRenderer.sortingLayerID : 0;
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

        // Move Out: Hidden -> Visible
        yield return StartCoroutine(MoveSmooth(_localStartPoint, _localEndPoint, hiddenColor, visibleColor, moveDuration));
        
        yield return new WaitForSeconds(peekStayTime);

        // Move In: Visible -> Hidden
        yield return StartCoroutine(MoveSmooth(_localEndPoint, _localStartPoint, visibleColor, hiddenColor, moveDuration));

        _isBusy = false;
        _myRenderer.enabled = false;
    }

    private IEnumerator MoveSmooth(Vector3 fromPos, Vector3 toPos, Color fromColor, Color toColor, float duration)
    {
        float elapsed = 0;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            t = t * t * (3f - 2f * t); 
            
            transform.localPosition = Vector3.Lerp(fromPos, toPos, t);
            _myRenderer.color = Color.Lerp(fromColor, toColor, t);
            
            yield return null;
        }
        transform.localPosition = toPos;
        _myRenderer.color = toColor;
    }

    private void OnDrawGizmosSelected()
    {
        Vector3 startPos = transform.localPosition + (transform.localRotation * startOffset);
        Vector3 endPos = transform.localPosition + (transform.localRotation * peekOffset);
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(startPos, endPos);
    }
}