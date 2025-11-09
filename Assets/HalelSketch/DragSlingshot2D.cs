using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem; // keep this semicolon!
#endif

[DisallowMultipleComponent]
[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class DragSlingshot2D : MonoBehaviour
{
    public enum LaunchDirection { TowardHome, AwayFromHome }

    [Header("Core")]
    public Rigidbody2D rb;
    public float dragZ = 0f;
    public bool kinematicWhileDragging = true;

    [Header("Drag (MoveTowards + collision cast)")]
    [Tooltip("Max world-units per FixedUpdate while dragging (how fast the object follows your pointer).")]
    public float dragUnitsPerStep = 1.5f;

    [Tooltip("Layers that block dragging movement.")]
    public LayerMask collisionMask = ~0;

    [Tooltip("Tiny buffer so we stop just before the obstacle.")]
    public float skin = 0.02f;

    [Header("Pull & Launch")]
    [Tooltip("Max pull distance around 'home'. 0 = unlimited.")]
    public float maxPullDistance = 3f;

    [Tooltip("Base speed per unit of pull distance.")]
    public float speedPerUnit = 8f;

    [Tooltip("Clamp final launch speed.")]
    public float minLaunchSpeed = 0f, maxLaunchSpeed = 20f;

    [Tooltip("Strength shaping over normalized pull (0..1).")]
    public AnimationCurve strengthCurve = AnimationCurve.Linear(0, 1, 1, 1);

    public LaunchDirection launchDirection = LaunchDirection.TowardHome;

    [Header("Re-arm (movement gating)")]
    [Tooltip("Speed below which the body is considered stopped.")]
    public float stopSpeedThreshold = 0.05f;

    [Tooltip("How long speed must remain below threshold to count as stopped.")]
    public float stopHoldTime = 0.12f;

    // --- internals ---
    private Camera _cam;
    private Collider2D _col;
    private bool _dragging;
    private bool _armed = true;
    private Vector3 _offsetWorld, _targetPos;
    private RigidbodyType2D _originalType;
    private Vector2 _home;            // decided ONLY at drag start
    private bool _hasHome = false;

    private float _stillTimer;
    private Vector2 _lastPos;

    // casting helpers
    private readonly RaycastHit2D[] _castHits = new RaycastHit2D[8];
    private ContactFilter2D _castFilter;

    void Awake()
    {
        _cam = Camera.main;
        rb ??= GetComponent<Rigidbody2D>();
        _col = GetComponent<Collider2D>();
        _originalType = rb.bodyType;

        _lastPos = rb.position;

        _castFilter = new ContactFilter2D { useLayerMask = true, useTriggers = false };
        _castFilter.SetLayerMask(collisionMask);

        if (maxPullDistance < 0f) maxPullDistance = 0f;
        if (maxLaunchSpeed < minLaunchSpeed) maxLaunchSpeed = minLaunchSpeed;
        if (dragUnitsPerStep < 0.01f) dragUnitsPerStep = 0.01f;
    }

    void Update()
    {
#if ENABLE_INPUT_SYSTEM
        var mouse = Mouse.current;
        if (mouse != null)
        {
            Vector3 world = ScreenToWorld(mouse.position.ReadValue());
            if (mouse.leftButton.wasPressedThisFrame) TryBeginDrag(world);
            if (_dragging && mouse.leftButton.isPressed) UpdateDrag(world);
            if (_dragging && mouse.leftButton.wasReleasedThisFrame) EndDrag();
        }
#else
        if (Input.GetMouseButtonDown(0))  TryBeginDrag(ScreenToWorld(Input.mousePosition));
        if (_dragging && Input.GetMouseButton(0)) UpdateDrag(ScreenToWorld(Input.mousePosition));
        if (_dragging && Input.GetMouseButtonUp(0)) EndDrag();
#endif
    }

    void FixedUpdate()
    {
        if (_dragging)
        {
            // --- MoveTowards with collision clamp ---
            Vector2 current = rb.position;
            Vector2 desired = _targetPos;
            Vector2 toTarget = desired - current;
            float distToTarget = toTarget.magnitude;

            if (distToTarget > 0.0001f)
            {
                Vector2 dir = toTarget / distToTarget;

                float step = Mathf.Min(distToTarget, dragUnitsPerStep);
                float allowed = step;

                int hitCount = _col.Cast(dir, _castFilter, _castHits, step + skin);
                if (hitCount > 0)
                {
                    float minHit = float.MaxValue;
                    for (int i = 0; i < hitCount; i++)
                    {
                        var h = _castHits[i];
                        if (!h.collider || h.collider.isTrigger) continue;
                        if (h.distance < minHit) minHit = h.distance;
                    }
                    if (minHit < float.MaxValue)
                        allowed = Mathf.Max(0f, minHit - skin);
                }

                Vector2 next = current + dir * allowed;
                rb.MovePosition(next);
            }
            return;
        }

        // Re-arm when truly stopped (do NOT change _home here)
        Vector2 curr = rb.position;
        float speed = (curr - _lastPos).magnitude / Mathf.Max(Time.fixedDeltaTime, 1e-6f);
        _lastPos = curr;

        if (!_armed)
        {
            if (speed <= stopSpeedThreshold)
            {
                _stillTimer += Time.fixedDeltaTime;
                if (_stillTimer >= stopHoldTime)
                {
                    _armed = true;
                    _stillTimer = 0f;
                    // _home remains whatever it was at the last TryBeginDrag()
                    // Next drag will choose a new home again.
                }
            }
            else _stillTimer = 0f;
        }
    }

    // --- drag logic ---
    Vector3 ScreenToWorld(Vector3 screen)
    {
        if (_cam == null) _cam = Camera.main;
        screen.z = Mathf.Abs(_cam.transform.position.z - dragZ);
        Vector3 world = _cam.ScreenToWorldPoint(screen);
        world.z = dragZ;
        return world;
    }

    void TryBeginDrag(Vector3 pointerWorld)
    {
        if (!_armed) return;
        if (_col != null && !_col.OverlapPoint(pointerWorld)) return;

        _dragging = true;
        _offsetWorld = transform.position - pointerWorld;

        // Decide anchor ('home') ONLY now
        _home = rb.position;
        _hasHome = true;

        if (kinematicWhileDragging)
        {
            _originalType = rb.bodyType;
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }
    }

    void UpdateDrag(Vector3 pointerWorld)
    {
        Vector3 desired = pointerWorld + _offsetWorld;
        desired.z = dragZ;

        if (_hasHome && maxPullDistance > 0f)
        {
            Vector2 fromHome = (Vector2)desired - _home;
            float dist = fromHome.magnitude;
            if (dist > maxPullDistance)
                desired = _home + fromHome.normalized * maxPullDistance;
        }

        _targetPos = desired;
    }

    void EndDrag()
    {
        _dragging = false;
        if (kinematicWhileDragging) rb.bodyType = _originalType;

        // Launch computed from the home set at TryBeginDrag()
        if (_hasHome)
        {
            Vector2 fromHome = rb.position - _home;
            float pullDist = fromHome.magnitude;

            if (pullDist > 0f)
            {
                float norm = (maxPullDistance > 0f) ? Mathf.Clamp01(pullDist / maxPullDistance) : 1f;
                float curve = Mathf.Max(0f, strengthCurve.Evaluate(norm));
                float speed = Mathf.Clamp(pullDist * speedPerUnit * curve, minLaunchSpeed, maxLaunchSpeed);

                Vector2 dir = (launchDirection == LaunchDirection.TowardHome)
                    ? -fromHome.normalized
                    :  fromHome.normalized;

                rb.linearVelocity = dir * speed;
            }
        }

        _armed = false;       // untouchable until it settles
        _stillTimer = 0f;
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        if (maxPullDistance > 0f && Application.isPlaying && _hasHome)
        {
            Gizmos.color = new Color(0f, 0.6f, 1f, 0.3f);
            Gizmos.DrawWireSphere(_home, maxPullDistance);
        }
    }
#endif
}