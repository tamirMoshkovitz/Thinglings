using System.Reflection;
using UnityEngine;
// חובה בשביל לגשת למשתנים פרטיים

// השם של ה-Namespace שלך

namespace _SLIME.LittleBoss
{
    public class LittleBossDebugger : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private Animator animator;
        [SerializeField] private Color textColor = Color.yellow;
        [SerializeField] private int fontSize = 24;
        [SerializeField] private Vector2 textPosition = new Vector2(20, 20);

        // משתנים לשמירת המידע שנשלוף
        private LittleBossMovement _movementState;
        private object _logicInstance;
    
        // שדות ה-Reflection שנשמור כדי לא לחפש כל פריים (משפר ביצועים)
        private FieldInfo _logicFieldInfo;
        private FieldInfo _chanceFieldInfo;
        private FieldInfo _timeStateFieldInfo;
        private FieldInfo _timerMoveFieldInfo;

        void Start()
        {
            if (animator == null) animator = GetComponent<Animator>();

            // 1. משיגים את ה-State מהאנימטור
            _movementState = animator.GetBehaviour<LittleBossMovement>();

            if (_movementState != null)
            {
                // 2. מכינים את הגישה למשתנה _logic שנמצא בתוך ה-State
                // BindingFlags.NonPublic | BindingFlags.Instance אומר "תביא לי גם פרייבט"
                _logicFieldInfo = typeof(LittleBossMovement).GetField("_logic", BindingFlags.NonPublic | BindingFlags.Instance);

                // 3. מכינים את הגישה למשתנים בתוך ה-Logic עצמו
                System.Type logicType = typeof(LittleBossMovementLogic);
                _chanceFieldInfo = logicType.GetField("_currentChance", BindingFlags.NonPublic | BindingFlags.Instance);
                _timeStateFieldInfo = logicType.GetField("_timeInMoveState", BindingFlags.NonPublic | BindingFlags.Instance);
                _timerMoveFieldInfo = logicType.GetField("_timerForMove", BindingFlags.NonPublic | BindingFlags.Instance);
            }
        }

        void OnGUI()
        {
            if (_movementState == null || _logicFieldInfo == null) return;

            // שלב א': שולפים את המופע הנוכחי של ה-Logic (כי הוא נוצר מחדש ב-Init)
            // אנחנו עושים את זה כל פריים למקרה שהלוגיקה הוחלפה או אותחלה מחדש
            _logicInstance = _logicFieldInfo.GetValue(_movementState);

            if (_logicInstance == null) return;

            // שלב ב': שולפים את הערכים מתוך הלוגיקה
            float currentChance = (float)_chanceFieldInfo.GetValue(_logicInstance);
            float timeInState = (float)_timeStateFieldInfo.GetValue(_logicInstance);
            float moveTimer = (float)_timerMoveFieldInfo.GetValue(_logicInstance);

            // שלב ג': מציירים על המסך
            GUIStyle style = new GUIStyle();
            style.fontSize = fontSize;
            style.normal.textColor = textColor;
            style.fontStyle = FontStyle.Bold;

            string debugText = $"--- Little Boss Debug ---\n" +
                               $"Current Chance: {currentChance:F4} ({currentChance * 100:F1}%)\n" +
                               $"Time In State: {timeInState:F2}\n" +
                               $"Move Loop Timer: {moveTimer:F2}";

            // ציור רקע שחור קטן כדי שיהיה קריא
            GUI.Box(new Rect(textPosition.x - 5, textPosition.y - 5, 400, 120), "");
            GUI.Label(new Rect(textPosition.x, textPosition.y, 400, 200), debugText, style);
        }
    }
}