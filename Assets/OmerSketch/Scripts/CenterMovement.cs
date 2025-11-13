using UnityEngine;
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
using UnityEngine.InputSystem;
#endif

[RequireComponent(typeof(Rigidbody2D))]
public class CenterMovement : MonoBehaviour
{
    public enum InputScheme { Arrows, WASD }
    public InputScheme inputScheme = InputScheme.WASD;
    public float speed = 5f;

    private Rigidbody2D _rb;

    void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
    }

    void FixedUpdate()
    {
        Vector2 dir = Vector2.zero;

        if (inputScheme == InputScheme.Arrows)
        {
            dir.x = (KeyIsPressed(KeyCode.RightArrow) ? 1f : 0f) - (KeyIsPressed(KeyCode.LeftArrow) ? 1f : 0f);
            dir.y = (KeyIsPressed(KeyCode.UpArrow) ? 1f : 0f) - (KeyIsPressed(KeyCode.DownArrow) ? 1f : 0f);
        }
        else // WASD
        {
            dir.x = (KeyIsPressed(KeyCode.D) ? 1f : 0f) - (KeyIsPressed(KeyCode.A) ? 1f : 0f);
            dir.y = (KeyIsPressed(KeyCode.W) ? 1f : 0f) - (KeyIsPressed(KeyCode.S) ? 1f : 0f);
        }

        if (dir.sqrMagnitude > 1f) dir = dir.normalized;

        Vector2 target = _rb.position + dir * (speed * Time.fixedDeltaTime);
        _rb.MovePosition(target);
    }

    private bool KeyIsPressed(KeyCode code)
    {
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
        if (Keyboard.current == null) return false;
        switch (code)
        {
            case KeyCode.W: return Keyboard.current.wKey.isPressed;
            case KeyCode.A: return Keyboard.current.aKey.isPressed;
            case KeyCode.S: return Keyboard.current.sKey.isPressed;
            case KeyCode.D: return Keyboard.current.dKey.isPressed;
            case KeyCode.UpArrow: return Keyboard.current.upArrowKey.isPressed;
            case KeyCode.DownArrow: return Keyboard.current.downArrowKey.isPressed;
            case KeyCode.LeftArrow: return Keyboard.current.leftArrowKey.isPressed;
            case KeyCode.RightArrow: return Keyboard.current.rightArrowKey.isPressed;
            default: return false;
        }
#else
        return Input.GetKey(code);
#endif
    }
}