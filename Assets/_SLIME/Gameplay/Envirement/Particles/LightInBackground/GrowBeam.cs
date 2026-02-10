using UnityEngine;

public class GrowBeam : MonoBehaviour
{
    [Header("הגדרות פתיחה")]
    [Tooltip("הזמן בשניות שלוקח לאור להגיע עד למטה")]
    public float growTime = 1.5f;

    [Header("הגדרות תנועה (Sway)")]
    [Tooltip("כמה מהר האור זז ימינה ושמאלה")]
    public float swaySpeed = 1.0f; 
    [Tooltip("המרחק המקסימלי שהאור זז מהמרכז")]
    public float swayDistance = 0.5f;
    [Tooltip("הוסף גיוון כדי שלא כל האורות יזוזו באותו סנכרון")]
    public float randomOffset = 0f;

    private LineRenderer line;
    private Vector3 startPoint; // התקרה
    private Vector3 endPointBase; // הרצפה (המיקום המקורי)
    private float initialWidthMultiplier; 
    private float timer;

    void Awake()
    {
        line = GetComponent<LineRenderer>();
        
        // שמירת המיקומים המקוריים
        startPoint = line.GetPosition(0); 
        endPointBase = line.GetPosition(1);
        initialWidthMultiplier = line.widthMultiplier;

        // אם לא הגדרת אופסט ידני, ניצור אחד רנדומלי כדי שזה ייראה טבעי
        if (randomOffset == 0) randomOffset = Random.Range(0f, 10f);
    }

    void OnEnable()
    {
        timer = 0f;
        line.SetPosition(1, startPoint);
        line.widthMultiplier = 0f; 
    }

    void Update()
    {
        // 1. חישוב הנדנוד (תמיד קורה, גם בזמן הגדילה וגם אחרי)
        // משתמשים ב-Sinus בשילוב עם הזמן כדי לקבל מספר שעולה ויורד בין -1 ל 1
        float swayX = Mathf.Sin((Time.time + randomOffset) * swaySpeed) * swayDistance;
        
        // יצירת נקודת יעד חדשה שהיא המקור + הסטייה של הנדנוד
        Vector3 targetWithSway = endPointBase + new Vector3(swayX, 0, 0);

        // 2. חישוב הגדילה (הפתיחה של האור)
        if (timer < growTime)
        {
            timer += Time.deltaTime;
            float progress = timer / growTime;
            float smoothProgress = Mathf.SmoothStep(0f, 1f, progress);

            // אינטרפולציה (הדרגה) בין ההתחלה ליעד המתנדנד
            Vector3 currentPos = Vector3.Lerp(startPoint, targetWithSway, smoothProgress);
            
            line.SetPosition(1, currentPos);
            line.widthMultiplier = Mathf.Lerp(0f, initialWidthMultiplier, smoothProgress);
        }
        else
        {
            // גם כשהגדילה הסתיימה, ממשיכים לעדכן את המיקום לפי הנדנוד
            line.SetPosition(1, targetWithSway);
            line.widthMultiplier = initialWidthMultiplier;
        }
    }
}