using _SLIME.BaseScripts;
using UnityEngine;

namespace _SLIME.Boss
{
    public class DestroyItem : ProjectMonoBehavior
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
}