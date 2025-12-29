using UnityEngine;

public class CheckConnection : MonoBehaviour
{
    void Start()
    {
        var myCollider = GetComponent<Collider2D>();
        if (myCollider.attachedRigidbody != null)
        {
            Debug.Log($"✅ מחובר! ה-Rigidbody שלי הוא: {myCollider.attachedRigidbody.name}");
        }
        else
        {
            Debug.LogError("❌ לא מחובר לשום Rigidbody!");
        }
    }
}