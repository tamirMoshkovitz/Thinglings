using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(ShowIfAttribute))]
public class ShowIfDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        // בדיקה אם התנאי מתקיים
        if (ShouldShow(property))
        {
            // אם כן, צייר את השדה כרגיל
            EditorGUI.PropertyField(position, property, label, true);
        }
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        // אם התנאי מתקיים, החזר את הגובה הרגיל. אם לא, החזר 0 (העלמה)
        if (ShouldShow(property))
        {
            return EditorGUI.GetPropertyHeight(property, label);
        }
        return -EditorGUIUtility.standardVerticalSpacing; // קיזוז רווחים קטנים
    }

    private bool ShouldShow(SerializedProperty property)
    {
        ShowIfAttribute attribute = (ShowIfAttribute)base.attribute;
        
        // חיפוש המשתנה הבוליאני שמשמש כתנאי
        // ה-Path מבטיח שזה יעבוד גם בתוך Structs ומערכים
        string path = property.propertyPath;
        string conditionPath = path.Replace(property.name, attribute.ConditionName);
        
        SerializedProperty conditionProp = property.serializedObject.FindProperty(conditionPath);

        if (conditionProp != null && conditionProp.propertyType == SerializedPropertyType.Boolean)
        {
            return conditionProp.boolValue;
        }

        return true; // ברירת מחדל: להציג אם לא נמצא התנאי
    }
}