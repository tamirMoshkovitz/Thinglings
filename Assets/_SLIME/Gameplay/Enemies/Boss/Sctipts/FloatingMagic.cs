using UnityEngine;
using DG.Tweening;

public class FloatingMagic : MonoBehaviour
{
    public float floatDistance = 0.5f; // How high relative to start
    public float duration = 1.5f;

    void Start()
    {
        // 1. Store the starting local Y so we know where "zero" is
        float startY = transform.localPosition.y;

        // 2. Move to (StartY + Distance) using Local Space
        transform.DOLocalMoveY(startY + floatDistance, duration)
            .SetLoops(-1, LoopType.Yoyo) // Infinite loop, back and forth
            .SetEase(Ease.InOutSine);    // Smooth "breathing" motion
    }
}