using UnityEngine;
using UnityEngine.SceneManagement;

namespace _SLIME.GameLoop
{
    public class AspectRatioManager : MonoBehaviour
    {
        [Header("Target Aspect Ratio")]
        public float targetAspectX = 16.0f;
        public float targetAspectY = 9.0f;

        // משתנים שישמרו את הגודל האחרון שזיהינו כדי שנוכל להשוות
        private int _lastWidth;
        private int _lastHeight;
        
        // נשמור רפרנס למצלמה כדי לא לחפש אותה כל פריים (אופטימיזציה)
        private Camera _cachedCamera;

        private void OnEnable()
        {
            SceneManager.activeSceneChanged += OnActiveSceneChanged;
        }

        private void OnDisable()
        {
            SceneManager.activeSceneChanged -= OnActiveSceneChanged;
        }

        private void Start()
        {
            // הרצה ראשונית
            RefreshCameraReference();
            ApplyCameraRatio();
        }

        private void Update()
        {
            // בדיקה כל פריים: האם גודל המסך השתנה?
            if (Screen.width != _lastWidth || Screen.height != _lastHeight)
            {
                ApplyCameraRatio();
            }
            
            // במקרה נדיר שהמצלמה התחלפה בלי החלפת סצנה (למשל השחקן מת ונוצרה מצלמה חדשה)
            // וודא שיש לנו מצלמה תקינה
            if (_cachedCamera == null)
            {
                RefreshCameraReference();
                if (_cachedCamera != null) ApplyCameraRatio();
            }
        }

        private void OnActiveSceneChanged(Scene current, Scene next)
        {
            RefreshCameraReference();
            ApplyCameraRatio();
        }
        
        private void RefreshCameraReference()
        {
             _cachedCamera = Camera.main;
        }

        private void ApplyCameraRatio()
        {
            // עדכון הרזולוציה האחרונה המוכרת
            _lastWidth = Screen.width;
            _lastHeight = Screen.height;

            // שימוש במצלמה השמורה. אם אין כזאת, מנסים למצוא שוב
            if (_cachedCamera == null) _cachedCamera = Camera.main;
            if (_cachedCamera == null) return;

            float targetAspect = targetAspectX / targetAspectY;
            float windowAspect = (float)Screen.width / (float)Screen.height;
            float scaleHeight = windowAspect / targetAspect;

            // איפוס לפני חישוב
            _cachedCamera.rect = new Rect(0, 0, 1, 1);

            if (scaleHeight < 1.0f) // הוספת פסים למעלה ולמטה (Letterbox)
            {
                Rect rect = _cachedCamera.rect;
                rect.width = 1.0f;
                rect.height = scaleHeight;
                rect.x = 0;
                rect.y = (1.0f - scaleHeight) / 2.0f;
                _cachedCamera.rect = rect;
            }
            else // הוספת פסים בצדדים (Pillarbox)
            {
                float scaleWidth = 1.0f / scaleHeight;
                Rect rect = _cachedCamera.rect;
                rect.width = scaleWidth;
                rect.height = 1.0f;
                rect.x = (1.0f - scaleWidth) / 2.0f;
                rect.y = 0;
                _cachedCamera.rect = rect;
            }
        }
    }
}