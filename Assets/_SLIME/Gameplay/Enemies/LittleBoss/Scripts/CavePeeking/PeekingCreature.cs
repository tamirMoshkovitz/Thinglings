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

    // These are set by the Manager, but visible here for debug
    [Header("Current State (Read Only)")]
    public Color visibleColor = Color.white; 
    public Color hiddenColor = Color.black;

    private SpriteRenderer _myRenderer;
    private SortingGroup _parentGroup;
    private SpriteRenderer _parentRenderer;
    
    private Vector3 _anchorPosition; // The "Center" position from the editor
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

        // 1. Capture the "Anchor" position (where you placed it in the scene)
        // We assume the object is placed at its "resting" center in the editor.
        _anchorPosition = transform.localPosition;

        // 2. Immediately snap to the start offset so it starts hidden
        ResetPosition();
        
        _myRenderer.enabled = false; 
    }

    void LateUpdate()
    {
        // Sync sorting order
        int order = GetTargetSortingOrder();
        _myRenderer.sortingLayerID = GetTargetSortingLayerID();
        _myRenderer.sortingOrder = order;
    }
    
    // Call this if you move the object in the editor and want to reset the anchor
    [ContextMenu("Reset Anchor")]
    public void ResetAnchor()
    {
        _anchorPosition = transform.localPosition;
    }

    private void ResetPosition()
    {
        // Calculate the start point based on current rotation
        Vector3 startPoint = _anchorPosition + (transform.localRotation * startOffset);
        transform.localPosition = startPoint;
        _myRenderer.color = hiddenColor;
    }

    public int GetTargetSortingOrder()
    {
        if (_parentGroup) return _parentGroup.sortingOrder - 1;
        if (_parentRenderer) return _parentRenderer.sortingOrder - 1;
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

        // 1. Calculate points NOW (using current rotation)
        // This fixes the issue: if the object rotated, the path rotates with it.
        Vector3 currentStart = _anchorPosition + (transform.localRotation * startOffset);
        Vector3 currentEnd   = _anchorPosition + (transform.localRotation * peekOffset);

        // Move Out
        yield return StartCoroutine(MoveSmooth(currentStart, currentEnd, hiddenColor, visibleColor, moveDuration));
        
        yield return new WaitForSeconds(peekStayTime);

        // Recalculate again on the way back just in case it rotated WHILE peeking
        currentStart = _anchorPosition + (transform.localRotation * startOffset);
        currentEnd   = _anchorPosition + (transform.localRotation * peekOffset);

        // Move In
        yield return StartCoroutine(MoveSmooth(currentEnd, currentStart, visibleColor, hiddenColor, moveDuration));

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
        // Use the current position as anchor if not playing, or the cached anchor if playing
        Vector3 basePos = Application.isPlaying ? _anchorPosition : transform.localPosition;
        
        // Calculate dynamic points for Gizmos
        Vector3 startPos = basePos + (transform.localRotation * startOffset);
        Vector3 endPos = basePos + (transform.localRotation * peekOffset);
        
        // Draw in World Space to visualize correct rotation
        Gizmos.matrix = transform.parent != null ? transform.parent.localToWorldMatrix : Matrix4x4.identity;

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(startPos, 0.1f);
        
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(endPos, 0.1f);
        
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(startPos, endPos);
    }
}