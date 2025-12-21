using _SLIME.Utils;
using UnityEngine;
using UnityEditor;
using DG.Tweening;

[CustomPropertyDrawer(typeof(TweenDefinition))]
public class TweenDefinitionDrawer : PropertyDrawer
{
    const float ROW_HEIGHT = 18f;
    const float PADDING = 2f;

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        Rect foldoutRect = new Rect(position.x, position.y, position.width, ROW_HEIGHT);
        property.isExpanded = EditorGUI.Foldout(foldoutRect, property.isExpanded, label);

        if (property.isExpanded)
        {
            EditorGUI.indentLevel++;
            
            var easeProp = property.FindPropertyRelative("easeType");
            var curveProp = property.FindPropertyRelative("curve");

            // --- 1. ציור ה-Ease Dropdown ---
            Rect easeRect = new Rect(position.x, position.y + ROW_HEIGHT + PADDING, position.width, ROW_HEIGHT);
            
            EditorGUI.BeginChangeCheck();
            EditorGUI.PropertyField(easeRect, easeProp, new GUIContent("Ease Type"));
            bool easeChanged = EditorGUI.EndChangeCheck();

            // --- 2. ציור ה-Curve ---
            Rect curveRect = new Rect(position.x, easeRect.y + ROW_HEIGHT + PADDING, position.width, ROW_HEIGHT);
            
            // טריק: אנחנו מאפשרים לערוך (כדי שאפשר יהיה לפתוח את החלון)
            // אבל אנחנו בודקים אם נעשה שינוי ידני
            EditorGUI.BeginChangeCheck();
            EditorGUI.PropertyField(curveRect, curveProp, new GUIContent("Visual Curve"));
            bool curveManuallyChanged = EditorGUI.EndChangeCheck();

            // --- 3. לוגיקת האכיפה (Enforcement) ---
            
            // אם שינו את ה-Ease ב-Dropdown -> חייבים לעדכן את הגרף
            if (easeChanged)
            {
                UpdateCurve(curveProp, (Ease)easeProp.enumValueIndex);
            }
            // אם המשתמש ניסה להזיז נקודה בגרף ידנית -> אנחנו דורסים את זה מיד!
            else if (curveManuallyChanged)
            {
                UpdateCurve(curveProp, (Ease)easeProp.enumValueIndex);
                // אופציונלי: להדפיס הודעה שהגרף מנוהל ע"י ה-Ease
                // Debug.Log("Curve is controlled by Ease type and cannot be edited manually.");
            }

            EditorGUI.indentLevel--;
        }

        EditorGUI.EndProperty();
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return property.isExpanded ? (ROW_HEIGHT * 3) + (PADDING * 2) : ROW_HEIGHT;
    }

    private void UpdateCurve(SerializedProperty curveProp, Ease selectedEase)
    {
        int resolution = 30;
        Keyframe[] keys = new Keyframe[resolution + 1];

        for (int i = 0; i <= resolution; i++)
        {
            float time = i / (float)resolution;
            float value = DOVirtual.EasedValue(0f, 1f, time, selectedEase);
            keys[i] = new Keyframe(time, value);
        }

        curveProp.animationCurveValue = new AnimationCurve(keys);
        
        // חשוב מאוד כדי שהשינוי "יתפוס" מיד
        curveProp.serializedObject.ApplyModifiedProperties();
    }
}