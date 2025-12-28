using UnityEngine;
using System.Collections.Generic;

namespace _SLIME.Utils
{
    [ExecuteAlways]
    public class InspectorLocker : MonoBehaviour
    {
        [Header("Select Scripts to Lock")]
        [Tooltip("גרור לכאן את הקומפוננטות שאתה רוצה לנעול")]
        public MonoBehaviour[] targetScripts; // שיניתי למערך

        // רשימה שתשמור את ההיסטוריה של מה שנעלנו, כדי שנוכל לשחרר אם הוא יוסר מהרשימה הראשית
        private List<MonoBehaviour> _lockedHistory = new List<MonoBehaviour>();

        void Update()
        {
            if (targetScripts == null) return;

            // שלב 1: זיהוי הסרות
            // אנחנו עוברים על ההיסטוריה. אם יש שם משהו שכבר לא נמצא ברשימה החדשה של המשתמש -> נשחרר אותו
            for (int i = _lockedHistory.Count - 1; i >= 0; i--)
            {
                MonoBehaviour script = _lockedHistory[i];

                // אם הסקריפט נמחק לגמרי או שהוא פשוט הוצא מהרשימה targetScripts
                if (script == null || !IsInCurrentList(script))
                {
                    Unlock(script);
                    _lockedHistory.RemoveAt(i);
                }
            }

            // שלב 2: נעילה והוספה להיסטוריה
            foreach (var script in targetScripts)
            {
                // הגנות: שלא יהיה ריק ושלא ינעל את עצמו
                if (script == null || script == this) continue;

                // אם הוא עדיין לא נעול - ננעל אותו
                if (script.hideFlags != HideFlags.NotEditable)
                {
                    script.hideFlags = HideFlags.NotEditable;
                }

                // אם הוא לא בהיסטוריה שלנו - נוסיף אותו
                if (!_lockedHistory.Contains(script))
                {
                    _lockedHistory.Add(script);
                }
            }
        }

        // פונקציית עזר לבדוק אם סקריפט קיים ברשימה הנוכחית
        private bool IsInCurrentList(MonoBehaviour scriptToCheck)
        {
            foreach (var s in targetScripts)
            {
                if (s == scriptToCheck) return true;
            }
            return false;
        }

        void OnDisable()
        {
            // שחרור של כל מה שברשימת ההיסטוריה
            foreach (var script in _lockedHistory)
            {
                Unlock(script);
            }
            _lockedHistory.Clear();
        }

        void Unlock(MonoBehaviour target)
        {
            if (target != null)
            {
                target.hideFlags = HideFlags.None;
            }
        }
    }
}