using Unity.VisualScripting;
using UnityEngine;

public class DestroyItem : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag($"Wall"))
        {
            Destroy(gameObject);
        }
    }
}
