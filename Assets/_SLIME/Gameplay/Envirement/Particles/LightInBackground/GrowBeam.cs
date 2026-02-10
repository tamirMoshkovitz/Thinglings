using UnityEngine;

public class GrowBeam : MonoBehaviour
{
    [Header("Position & Follow")]
    [Tooltip("Drag here the object you want the light beam to follow (e.g., the boss)")]
    public Transform targetToFollow;
    [Tooltip("Offset of the light from the target (usually Y is higher)")]
    public Vector3 offset = new Vector3(0, 5, 0);

    [Header("Opening Animation")]
    public float growTime = 1.5f;

    [Header("Sway Animation")]
    public float swaySpeed = 1.0f;
    public float swayDistance = 0.5f;
    public float randomOffset = 0f;

    private LineRenderer line;
    private Vector3 startPointLocal; // נקודה עליונה (לוקאלית)
    private Vector3 endPointLocalBase; // נקודה תחתונה מקורית (לוקאלית)
    private float initialWidthMultiplier;
    private float timer;

    void Awake()
    {
        line = GetComponent<LineRenderer>();
        
        // מוודאים שהקו עובד במרחב לוקאלי כדי שיזוז עם האובייקט
        line.useWorldSpace = false;

        // שומרים את הנקודות כפי שציירת אותן בסצנה
        startPointLocal = line.GetPosition(0);
        endPointLocalBase = line.GetPosition(1);
        initialWidthMultiplier = line.widthMultiplier;

        if (randomOffset == 0) randomOffset = Random.Range(0f, 10f);
    }

    void OnEnable()
    {
        timer = 0f;
        line.SetPosition(1, startPointLocal);
        line.widthMultiplier = 0f;
    }

    void Update()
    {
        // --- חלק 1: מעקב אחרי האובייקט ---
        if (targetToFollow != null)
        {
            // מזיזים את כל האובייקט של האור למיקום של המטרה + האופסט
            transform.position = targetToFollow.position + offset;
        }

        // --- חלק 2: חישוב הנדנוד ---
        float swayX = Mathf.Sin((Time.time + randomOffset) * swaySpeed) * swayDistance;
        
        // הנדנוד מחושב ביחס למיקום הלוקאלי
        Vector3 targetLocalPos = endPointLocalBase + new Vector3(swayX, 0, 0);

        // --- חלק 3: גדילה ועדכון הקו ---
        if (timer < growTime)
        {
            timer += Time.deltaTime;
            float progress = timer / growTime;
            float smoothProgress = Mathf.SmoothStep(0f, 1f, progress);

            // אינטרפולציה בין התקרה לרצפה המתנדנדת
            Vector3 currentPos = Vector3.Lerp(startPointLocal, targetLocalPos, smoothProgress);

            line.SetPosition(1, currentPos);
            line.widthMultiplier = Mathf.Lerp(0f, initialWidthMultiplier, smoothProgress);
        }
        else
        {
            // סיום הגדילה - רק נדנוד
            line.SetPosition(1, targetLocalPos);
            line.widthMultiplier = initialWidthMultiplier;
        }
    }
}