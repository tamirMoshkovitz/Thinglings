using System;
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;

// המטרה: העורך הזה יופעל על כל ScriptableObject באופן בסיסי, 
// אבל יצייר טאבים רק אם ימצא את ה-Attribute שלנו.
[CustomEditor(typeof(TabbedScriptableObject), true)] 
public class TabbedEditor : Editor
{
    // מילון שמחזיק את שמות הטאבים ואת רשימת המשתנים ששייכים להם
    private Dictionary<string, List<SerializedProperty>> _tabs;
    private string[] _tabNames;
    private int _selectedTabIndex = 0;
    private bool _hasTabs = false;

    private void OnEnable()
    {
        _tabs = new Dictionary<string, List<SerializedProperty>>();
        var otherProperties = new List<SerializedProperty>(); // למשתנים בלי טאב

        // השגת המשתנה עליו אנו מסתכלים כרגע
        SerializedProperty iterator = serializedObject.GetIterator();
        bool enterChildren = true;

        while (iterator.NextVisible(enterChildren))
        {
            enterChildren = false; // לא נכנסים לעומק המערכים/מחלקות כרגע

            if (iterator.name == "m_Script") continue; // דילוג על שדה הסקריפט

            // שימוש ב-Reflection כדי למצוא את ה-Attribute על השדה המקורי
            FieldInfo field = target.GetType().GetField(iterator.name, 
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            if (field != null)
            {
                // בדיקה אם יש עליו את ה-Attribute שלנו
                var tabAttribute = (TabAttribute)Attribute.GetCustomAttribute(field, typeof(TabAttribute));

                if (tabAttribute != null)
                {
                    // אם יש טאב, נוסיף לרשימה המתאימה
                    if (!_tabs.ContainsKey(tabAttribute.tabName))
                    {
                        _tabs[tabAttribute.tabName] = new List<SerializedProperty>();
                    }
                    _tabs[tabAttribute.tabName].Add(serializedObject.FindProperty(iterator.name));
                }
                else
                {
                    // משתנים בלי טאב
                    otherProperties.Add(serializedObject.FindProperty(iterator.name));
                }
            }
        }

        // אם מצאנו טאבים, נכין את המערכים
        if (_tabs.Count > 0)
        {
            _hasTabs = true;
            if (otherProperties.Count > 0)
            {
                _tabs["General"] = otherProperties;
            }
            _tabNames = _tabs.Keys.ToArray();
        }
    }

    public override void OnInspectorGUI()
    {
        if (!_hasTabs)
        {
            DrawDefaultInspector();
            return;
        }

        serializedObject.Update();
        
        EditorGUILayout.Space();
        _selectedTabIndex = GUILayout.Toolbar(_selectedTabIndex, _tabNames);
        EditorGUILayout.Space();

       
        string selectedTabName = _tabNames[_selectedTabIndex];
        if (_tabs.TryGetValue(selectedTabName, out List<SerializedProperty> properties))
        {
            foreach (var prop in properties)
            {
                EditorGUILayout.PropertyField(prop, true);
            }
        }

        serializedObject.ApplyModifiedProperties();
    }
}