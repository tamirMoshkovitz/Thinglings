using UnityEngine;
using _SLIME.Projectiles;

namespace _SLIME.DebugTools
{
    [RequireComponent(typeof(LineRenderer))] // מוודא שיש LineRenderer
    public class BulletDebugger : MonoBehaviour
    {
        [Header("Setup")]
        public Bullet bulletScript;
        
        [Header("Visual Settings")]
        public Color lineColor = Color.red;
        public float lineWidth = 0.05f;
        public Vector2 textOffset = new Vector2(20, -20); // הזזה של הטקסט ליד הכדור

        private LineRenderer _lineRenderer;
        private GUIStyle _guiStyle;

        private void Start()
        {
            if (bulletScript == null) bulletScript = GetComponent<Bullet>();

            // הגדרת הקו (LineRenderer) בצורה אוטומטית
            _lineRenderer = GetComponent<LineRenderer>();
            _lineRenderer.startWidth = lineWidth;
            _lineRenderer.endWidth = lineWidth;
            _lineRenderer.material = new Material(Shader.Find("Sprites/Default")); // חומר בסיסי שנראה תמיד
            _lineRenderer.startColor = lineColor;
            _lineRenderer.endColor = lineColor;
            _lineRenderer.positionCount = 2;
        }

        private void Update()
        {
            // אם הכדור לא פעיל או אין לו מטרה, אל תצייר קו
            if (bulletScript == null || !bulletScript.DebugIsActive || bulletScript.DebugData.target == null)
            {
                _lineRenderer.enabled = false;
                return;
            }

            // עדכון הקו: ממיקום הכדור -> למיקום המטרה
            _lineRenderer.enabled = true;
            _lineRenderer.SetPosition(0, transform.position);
            _lineRenderer.SetPosition(1, bulletScript.DebugData.target.position);
        }

        // פונקציה מיוחדת של יוניטי לציור ממשק משתמש (UI) פשוט ומהיר
        private void OnGUI()
        {
            if (bulletScript == null || !bulletScript.DebugIsActive) return;

            SetupStyleIfNeeded();

            // המרת מיקום הכדור בעולם למיקום בפיקסלים על המסך
            Vector3 screenPos = Camera.main.WorldToScreenPoint(transform.position);

            // אם הכדור מאחורי המצלמה, אל תצייר טקסט
            if (screenPos.z < 0) return;

            // תיקון קואורדינטות (OnGUI עובד הפוך בציר Y)
            float uiY = Screen.height - screenPos.y; 

            // בניית המידע להצגה
            string debugText = "TARGET: " + (bulletScript.DebugData.target != null ? bulletScript.DebugData.target.name : "NULL") + "\n" +
                               "SPEED: " + bulletScript.DebugData.speed + "\n" +
                               "VELOCITY: " + bulletScript.DebugRB.linearVelocity.magnitude.ToString("F1");

            // ציור הטקסט ליד הכדור
            Rect rect = new Rect(screenPos.x + textOffset.x, uiY + textOffset.y, 200, 100);
            GUI.Label(rect, debugText, _guiStyle);
        }

        // הגדרת עיצוב הטקסט (פעם אחת)
        private void SetupStyleIfNeeded()
        {
            if (_guiStyle == null)
            {
                _guiStyle = new GUIStyle();
                _guiStyle.fontSize = 14;
                _guiStyle.fontStyle = FontStyle.Bold;
                _guiStyle.normal.textColor = Color.yellow; // צבע הטקסט
                
                // הוספת רקע שחור לטקסט כדי שיהיה קריא (אופציונלי)
                Texture2D blackTex = new Texture2D(1, 1);
                blackTex.SetPixel(0, 0, new Color(0, 0, 0, 0.5f));
                blackTex.Apply();
                _guiStyle.normal.background = blackTex;
            }
        }
    }
}