using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(HSVColor))]
public class HSVColorDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        // התחלת ציור הנכס
        EditorGUI.BeginProperty(position, label, property);

        // שליפת המאפיינים הפנימיים
        SerializedProperty hProp = property.FindPropertyRelative("h");
        SerializedProperty sProp = property.FindPropertyRelative("s");
        SerializedProperty vProp = property.FindPropertyRelative("v");

        // יצירת מלבן עבור התווית (השם של המשתנה)
        Rect labelRect = new Rect(position.x, position.y, EditorGUIUtility.labelWidth, position.height);
        EditorGUI.LabelField(labelRect, label);

        // יצירת מלבן עבור שדה הצבע
        Rect contentRect = new Rect(position.x + EditorGUIUtility.labelWidth, position.y, position.width - EditorGUIUtility.labelWidth, position.height);

        // חישוב הצבע הנוכחי מתוך ערכי ה-HSV השמורים
        Color currentColor = Color.HSVToRGB(hProp.floatValue, sProp.floatValue, vProp.floatValue);

        // ציור שדה הצבע של יוניטי (עם תמיכה ב-Undo)
        EditorGUI.BeginChangeCheck();
        Color newColor = EditorGUI.ColorField(contentRect, GUIContent.none, currentColor, true, false, false);

        if (EditorGUI.EndChangeCheck())
        {
            // אם המשתמש שינה את הצבע, נמיר חזרה ל-HSV ונשמור
            float h, s, v;
            Color.RGBToHSV(newColor, out h, out s, out v);

            hProp.floatValue = h;
            sProp.floatValue = s;
            vProp.floatValue = v;
        }

        EditorGUI.EndProperty();
    }
}