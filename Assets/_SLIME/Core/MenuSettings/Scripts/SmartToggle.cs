using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems; // חובה בשביל לזהות אם אנחנו "על" הכפתור


namespace _SLIME.Core.MenuSettings.Scripts
{
    [RequireComponent(typeof(Toggle))]
    [RequireComponent(typeof(Image))]
    public class SmartToggle : MonoBehaviour, ISelectHandler, IDeselectHandler
    {
        [Header("הגדרות ויזואליות")]
        public Sprite baseSprite;        // ספרייט בסיסי (רגיל + היילייט)
        public Sprite activeSprite;      // ספרייט כשהכפתור נבחר
        [Space]
        public Color normalColor = Color.white;   // צבע לסטייט רגיל
        public Color activeColor = Color.white;   // צבע כשהכפתור נבחר
        public Color highlightColor = Color.white; // צבע להיילייט

        private Toggle toggle;
        private Image targetImage;
        private bool isHovering = false; 

        void Awake()
        {
            toggle = GetComponent<Toggle>();
            targetImage = GetComponent<Image>();
            toggle.onValueChanged.AddListener(OnToggleValueChanged);
        }

        void Start()
        {
            UpdateVisuals(); 
        }

        // כשבוחרים את הכפתור עם הג'ויסטיק
        public void OnSelect(BaseEventData eventData)
        {
            isHovering = true;
            UpdateVisuals();
        }

        // כשעוזבים את הכפתור עם הג'ויסטיק
        public void OnDeselect(BaseEventData eventData)
        {
            isHovering = false;
            UpdateVisuals();
        }

        // כשהערך של ה-Toggle משתנה (נבחר/לא נבחר)
        public void OnToggleValueChanged(bool isOn)
        {
            UpdateVisuals();
        }

        // המוח - מחליט איזה ספרייט להראות לפי סדר העדיפויות שביקשת
        void UpdateVisuals()
        {
            if (toggle.isOn && isHovering)
            {
                targetImage.sprite = activeSprite != null ? activeSprite : baseSprite;
                targetImage.color = highlightColor;
            }
            else if (toggle.isOn)
            {
                targetImage.sprite = activeSprite != null ? activeSprite : baseSprite;
                targetImage.color = activeColor;
            }
            else if (isHovering)
            {
                targetImage.sprite = baseSprite;
                targetImage.color = highlightColor;
            }
            else
            {
                targetImage.sprite = baseSprite;
                targetImage.color = normalColor;
            }
        }
    }
}