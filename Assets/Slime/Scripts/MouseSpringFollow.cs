using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Slime.Scripts
{
    /// <summary>
    /// Follows the mouse with a springy, slightly overshooting delay.
    /// Uses the new Input System for mouse position.
    /// </summary>
    public class MouseSpringFollow : MonoBehaviour
    {
        private static readonly int Speed = Animator.StringToHash("Speed");
        private static readonly int Stretch = Animator.StringToHash("Stretch");

        [Header("Spring Settings (tweak at runtime)")]
        [Tooltip("Natural frequency in Hz. Higher = snappier/faster.")]
        [SerializeField] private float frequency = 3.0f;

        [Range(0.0f, 1.0f)]
        [Tooltip("Damping ratio. < 1 = underdamped (overshoot), 1 = critically damped, >1 = overdamped.")]
        [SerializeField] private float dampingRatio = 0.25f;

        [Tooltip("How much to anticipate target motion (0 = none, 1 = strong).")]
        [SerializeField] private float responsiveness = 0.0f;

        [Header("Depth")]
        [Tooltip("If true, use the object's current depth from the camera for ScreenToWorldPoint.")]
        [SerializeField] private bool useObjectDepth = true;

        [Tooltip("If not using object depth, use this distance (in world units) from the camera.")]
        [SerializeField] private float fixedDepthFromCamera = 10f;
        
        [SerializeField] private Transform cielingHeight;

        private Vector3 _xPrev; // previous target (mouse) position
        private Vector3 _y;     // current output (our position)
        private Vector3 _yd;    // current output velocity

        private Vector3 _lastY;

        private Camera _cam;
        private Animator _animator;

        private bool _isStretching = false;
        private float _lastDesiredForceY = 0f;

        void Start()
        {
            _cam = Camera.main;
            _animator = GetComponent<Animator>();
            var x0 = GetMouseWorldPosition();
            _xPrev = x0;
            _y = transform.position;
            _lastY = _y;
            _yd = Vector3.zero;
        }

        void Update()
        {
            float dt = Time.deltaTime;
            if (dt <= 0f) return;
            dt = Mathf.Min(dt, 0.05f);

            // Always track target motion & spring terms
            Vector3 x = GetMouseWorldPosition();
            Vector3 xd = (x - _xPrev) / Mathf.Max(dt, 1e-5f); // input velocity estimate
            _xPrev = x;

            float f = Mathf.Max(0.01f, frequency);
            float z = Mathf.Max(0.0f, dampingRatio);
            float w = 2f * Mathf.PI * f;

            float k1 = 2f * z * w; // damping
            float k2 = w * w;      // spring
            float k  = responsiveness / Mathf.Max(1e-5f, w);

            // Compute desired spring force on Y (what the system *wants* to do)
            _lastDesiredForceY = (k2 * (x + k * xd - _y) - k1 * _yd).y;

            // If currently stretching, don't integrate or move, but keep force updated
            if (_isStretching)
                return;

            _yd += (k2 * (x + k * xd - _y) - k1 * _yd) * dt;
            _y  += _yd * dt;

            transform.position = _y;

            float acceleration = ((_y - _lastY).y) / Mathf.Max(dt, 1e-5f);
            _lastY = _y;

            if (_animator != null)
            {
                _animator.SetFloat(Speed, _yd.magnitude / 100f);
                if (acceleration > 75f && _yd.magnitude / 100f > .1f)
                {
                    _animator.SetBool(Stretch, true);
                    _isStretching = true;
                    
                    if (transform.position.y < cielingHeight.position.y)
                    {
                        Vector3 newPosition = new Vector3(
                            transform.position.x,
                            cielingHeight.position.y,
                            transform.position.z
                        );
                        transform.position = newPosition;
                        StartCoroutine(LockMovementForSeconds(1f));
                    }
                }
            }
        }

        private IEnumerator LockMovementForSeconds(float i)
        {
            yield return new WaitForSeconds(i);
            _animator.SetBool(Stretch, false);
            yield return new WaitForSeconds(0.2f);
            _isStretching = false;
        }

        private Vector3 GetMouseWorldPosition()
        {
            if (_cam == null || Mouse.current == null)
                return transform.position;

            Vector2 mouseScreen = Mouse.current.position.ReadValue();

            float depth;
            if (useObjectDepth)
            {
                depth = Mathf.Abs(_cam.WorldToScreenPoint(transform.position).z);
                if (depth <= 0.01f) depth = fixedDepthFromCamera;
            }
            else
            {
                depth = fixedDepthFromCamera;
            }

            Vector3 screenPos = new Vector3(mouseScreen.x, mouseScreen.y, depth);
            return _cam.ScreenToWorldPoint(screenPos);
        }

#if UNITY_EDITOR
        void OnValidate()
        {
            frequency = Mathf.Max(0.01f, frequency);
            dampingRatio = Mathf.Max(0f, dampingRatio);
        }
#endif
    }
}