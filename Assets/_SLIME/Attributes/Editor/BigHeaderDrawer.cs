using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(BigHeaderAttribute))]
public class BigHeaderDrawer : DecoratorDrawer
{
    // משתנה עזר לרווח בין הכותרת לבין השדה שמעליה
    private const float TOP_PADDING = 10f;
    // משתנה עזר לרווח בין הכותרת לבין השדה שמתחתיה
    private const float BOTTOM_PADDING = 5f;

    public override float GetHeight()
    {
        BigHeaderAttribute headerAttr = (BigHeaderAttribute)attribute;
        
        // הנוסחה החדשה: גודל הפונט + רווח עליון + רווח תחתון
        // זה מבטיח שהשדה הבא יידחף מספיק למטה
        return headerAttr.fontSize + TOP_PADDING + BOTTOM_PADDING;
    }

    public override void OnGUI(Rect position)
    {
        BigHeaderAttribute headerAttr = (BigHeaderAttribute)attribute;

        // הגדרת הסגנון
        GUIStyle style = new GUIStyle(EditorStyles.boldLabel);
        style.fontSize = headerAttr.fontSize;
        style.alignment = TextAnchor.LowerLeft; // מיישר את הטקסט למטה ושמאלה

        // --- לוגיקת הצבע (כמו מקודם) ---
        Color finalColor = Color.white;
        if (headerAttr.predefinedColor.HasValue)
        {
            finalColor = GetColorFromEnum(headerAttr.predefinedColor.Value);
        }
        else if (!string.IsNullOrEmpty(headerAttr.hexColor))
        {
            ColorUtility.TryParseHtmlString(headerAttr.hexColor, out finalColor);
        }
        style.normal.textColor = finalColor;

        // --- חישוב המיקום המדויק ---
        // אנחנו מזיזים את איזור הציור קצת למטה (לפי ה-TOP_PADDING)
        // ונותנים לו גובה בדיוק לפי גודל הפונט, כדי שלא יתפזר
        Rect labelRect = new Rect(
            position.x, 
            position.y + TOP_PADDING, 
            position.width, 
            headerAttr.fontSize + 5f
        );

        GUI.Label(labelRect, headerAttr.headerText, style);
    }

    private Color GetColorFromEnum(HeaderColor colorName)
    {
        switch (colorName)
        {
            case HeaderColor.Red: return new Color(1f, 0.4f, 0.4f); // אדום רך
            case HeaderColor.Green: return new Color(0.4f, 1f, 0.4f);
            case HeaderColor.Blue: return new Color(0.4f, 0.7f, 1f);
            case HeaderColor.Cyan: return Color.cyan;
            case HeaderColor.Yellow: return new Color(1f, 0.92f, 0.016f);
            case HeaderColor.Magenta: return Color.magenta;
            case HeaderColor.Grey: return Color.grey;
            case HeaderColor.Orange: return new Color(1f, 0.6f, 0f);
            case HeaderColor.Lime: return new Color(0.7f, 1f, 0f);
            case HeaderColor.Gold: return new Color(1f, 0.84f, 0f);
            case HeaderColor.Pink: return new Color(1f, 0.6f, 0.8f);
            default: return Color.white;
        }
    }
}