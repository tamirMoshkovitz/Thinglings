using UnityEditor;
using UnityEngine;
using _SLIME.Boss;

namespace _SLIME.Boss.Editor
{
    [CustomPropertyDrawer(typeof(AttackProbabilities))]
    public class AttackProbabilitiesDrawer : PropertyDrawer
    {
        private const float Spacing = 2f;

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight * 8 + Spacing * 7;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            var oneShotProp = property.FindPropertyRelative("oneShot");
            var twoShotsProp = property.FindPropertyRelative("twoShots");
            var threeShotProp = property.FindPropertyRelative("threeShot");
            var fourShotsProp = property.FindPropertyRelative("fourShots");
            var bulletHellProp = property.FindPropertyRelative("bulletHell");

            float y = position.y;
            float lineHeight = EditorGUIUtility.singleLineHeight;

            // Header Label
            EditorGUI.LabelField(new Rect(position.x, y, position.width, lineHeight), label, EditorStyles.boldLabel);
            y += lineHeight + Spacing;

            // Indent
            EditorGUI.indentLevel++;
            float indent = EditorGUI.indentLevel * 15f;

            // Draw each probability field using EditorGUI.Slider
            Rect fieldRect = new Rect(position.x, y, position.width, lineHeight);
            EditorGUI.Slider(fieldRect, oneShotProp, 0f, 1f, new GUIContent("One Shot"));
            y += lineHeight + Spacing;

            fieldRect = new Rect(position.x, y, position.width, lineHeight);
            EditorGUI.Slider(fieldRect, twoShotsProp, 0f, 1f, new GUIContent("Two Shots"));
            y += lineHeight + Spacing;

            fieldRect = new Rect(position.x, y, position.width, lineHeight);
            EditorGUI.Slider(fieldRect, threeShotProp, 0f, 1f, new GUIContent("Three Shot"));
            y += lineHeight + Spacing;

            fieldRect = new Rect(position.x, y, position.width, lineHeight);
            EditorGUI.Slider(fieldRect, fourShotsProp, 0f, 1f, new GUIContent("Four Shots"));
            y += lineHeight + Spacing;

            fieldRect = new Rect(position.x, y, position.width, lineHeight);
            EditorGUI.Slider(fieldRect, bulletHellProp, 0f, 1f, new GUIContent("Bullet Hell"));
            y += lineHeight + Spacing;

            // Calculate total
            float total = oneShotProp.floatValue + twoShotsProp.floatValue + 
                          threeShotProp.floatValue + fourShotsProp.floatValue + bulletHellProp.floatValue;

            // Total display row
            Rect totalRowRect = new Rect(position.x + indent, y, position.width - indent, lineHeight);
            float halfWidth = totalRowRect.width / 2f;

            // Total label with color
            Color originalColor = GUI.color;
            GUI.color = Mathf.Approximately(total, 1f) ? Color.green : Color.red;
            EditorGUI.LabelField(new Rect(totalRowRect.x, y, halfWidth, lineHeight), 
                $"Total: {total:F3}", EditorStyles.boldLabel);
            GUI.color = originalColor;

            // Normalize button
            Rect buttonRect = new Rect(totalRowRect.x + halfWidth, y, halfWidth, lineHeight);
            if (GUI.Button(buttonRect, "Normalize to 1"))
            {
                if (total > 0f)
                {
                    oneShotProp.floatValue /= total;
                    twoShotsProp.floatValue /= total;
                    threeShotProp.floatValue /= total;
                    fourShotsProp.floatValue /= total;
                    bulletHellProp.floatValue /= total;
                }
                else
                {
                    oneShotProp.floatValue = 1f;
                    twoShotsProp.floatValue = 0f;
                    threeShotProp.floatValue = 0f;
                    fourShotsProp.floatValue = 0f;
                    bulletHellProp.floatValue = 0f;
                }
            }

            EditorGUI.indentLevel--;
            EditorGUI.EndProperty();
        }
    }
}
