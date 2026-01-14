using System;
using System.Reflection;
using UnityEngine;
using _SLIME.Tutorial;
using DG.Tweening;

namespace _SLIME.Tutorial
{
    public class TutorialDebugger : MonoBehaviour
    {
        // Instances found in scene
        private TutorialStateManager _tutorialStateManager;
        private Rock _rock;
        
        // Reflection info for TutorialStateManager
        private PropertyInfo _currentStateProp;
        private FieldInfo _tutorialScriptableField;
        private FieldInfo _rockStateDepsField;
        private FieldInfo _riseToBossStateDepsField;
        private FieldInfo _spellHitStateDepsField;
        
        // Reflection info for Rock
        private FieldInfo _rockShakeSettingsField;
        private FieldInfo _isShakingField;
        private FieldInfo _accumulatedShakeTimeField;
        private FieldInfo _currentShakeTweenField;
        private FieldInfo _eventTriggeredField;
        
        // Reflection info for TutorialScriptable
        private PropertyInfo _rockShakeSettingsProp;
        private PropertyInfo _rockStateSetProp;
        private PropertyInfo _riseToBossStateSetProp;
        private PropertyInfo _spellHitStateSetProp;
        
        // For ScrollView
        private Vector2 _scrollPosition = Vector2.zero;
        
        private void Start()
        {
            FindInstancesInScene();
        }
        
        private void Update()
        {
            // Refresh if not found or every 100 frames to ensure things haven't changed
            if ((_tutorialStateManager == null || _rock == null) && Time.frameCount % 100 == 0)
            {
                FindInstancesInScene();
            }
        }
        
        private void FindInstancesInScene()
        {
            // Find TutorialStateManager
            _tutorialStateManager = FindObjectOfType<TutorialStateManager>();
            if (_tutorialStateManager != null)
            {
                InitTutorialStateManagerReflection();
            }
            
            // Find Rock
            _rock = FindObjectOfType<Rock>();
            if (_rock != null)
            {
                InitRockReflection();
            }
        }
        
        private void InitTutorialStateManagerReflection()
        {
            Type t = typeof(TutorialStateManager);
            _currentStateProp = t.GetProperty("CurrentState", BindingFlags.Instance | BindingFlags.Public);
            _tutorialScriptableField = t.GetField("tutorialScriptable", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            _rockStateDepsField = t.GetField("rockStateDeps", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            _riseToBossStateDepsField = t.GetField("riseToBossStateDeps", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            _spellHitStateDepsField = t.GetField("spellHitStateDeps", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        }
        
        private void InitRockReflection()
        {
            Type t = typeof(Rock);
            _rockShakeSettingsField = t.GetField("rockShakeSettings", BindingFlags.Instance | BindingFlags.NonPublic);
            _isShakingField = t.GetField("_isShaking", BindingFlags.Instance | BindingFlags.NonPublic);
            _accumulatedShakeTimeField = t.GetField("_accumulatedShakeTime", BindingFlags.Instance | BindingFlags.NonPublic);
            _currentShakeTweenField = t.GetField("_currentShakeTween", BindingFlags.Instance | BindingFlags.NonPublic);
            _eventTriggeredField = t.GetField("_eventTriggered", BindingFlags.Instance | BindingFlags.NonPublic);
        }
        
        private void InitTutorialScriptableReflection(object scriptableInstance)
        {
            if (scriptableInstance == null) return;
            Type t = scriptableInstance.GetType();
            _rockShakeSettingsProp = t.GetProperty("RockShakeSettings");
            _rockStateSetProp = t.GetProperty("RockStateSet");
            _riseToBossStateSetProp = t.GetProperty("RiseToBossStateSet");
            _spellHitStateSetProp = t.GetProperty("SpellHitStateSet");
        }
        
        private string GetStructFieldsString(object structInstance, Type structType, string prefix = "")
        {
            if (structInstance == null) return "";
            
            string result = "";
            var fields = structType.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            
            foreach (var field in fields)
            {
                try
                {
                    var value = field.GetValue(structInstance);
                    string valueStr = "null";
                    
                    if (value != null)
                    {
                        if (value is float f)
                            valueStr = $"{f:F2}";
                        else if (value is bool b)
                            valueStr = b ? "<color=green>TRUE</color>" : "<color=gray>FALSE</color>";
                        else if (value is int i)
                            valueStr = $"{i}";
                        else if (value is Transform transform)
                            valueStr = transform != null ? $"{transform.name} (Transform)" : "null";
                        else if (value is UnityEngine.Object obj)
                            valueStr = obj != null ? $"{obj.name} ({obj.GetType().Name})" : "null";
                        else
                            valueStr = value.ToString();
                    }
                    
                    result += $"{prefix}{field.Name}: <b>{valueStr}</b>\n";
                }
                catch { }
            }
            
            return result;
        }
        
        private void OnGUI()
        {
            if (_tutorialStateManager == null && _rock == null)
            {
                GUI.Label(new Rect(10, 10, 300, 20), "Scanning scene for Tutorial classes...");
                return;
            }
            
            GUIStyle style = new GUIStyle(GUI.skin.box);
            style.alignment = TextAnchor.UpperLeft;
            style.fontSize = 13;
            style.normal.textColor = new Color(0.9f, 0.9f, 0.9f);
            style.richText = true;
            
            string text = "<b><size=15>--- TUTORIAL DEBUGGER ---</size></b>\n";
            
            TutorialState currentState = TutorialState.RockState;
            if (_tutorialStateManager != null && _currentStateProp != null)
            {
                currentState = (TutorialState)_currentStateProp.GetValue(_tutorialStateManager);
            }
            
            text += "\n<color=#FFA500><b>[Tutorial State Manager]</b></color>\n";
            text += $"Current State: <color=yellow><b>{currentState}</b></color>\n";
            
            var scriptable = _tutorialScriptableField?.GetValue(_tutorialStateManager) as TutorialScriptable;
            if (scriptable != null)
            {
                InitTutorialScriptableReflection(scriptable);
            }
            
            // Display only current state details
            switch (currentState)
            {
                case TutorialState.RockState:
                    text += DisplayRockStateDetails(scriptable);
                    break;
                case TutorialState.RiseToBoss:
                    text += DisplayRiseToBossStateDetails(scriptable);
                    break;
                case TutorialState.SpellHit:
                    text += DisplaySpellHitStateDetails(scriptable);
                    break;
                default:
                    text += "\n<color=#FF69B4><b>[Current State]</b></color>\n";
                    text += $"<i>No specific debug info for state: {currentState}</i>\n";
                    break;
            }
            
            // Display the text in a scrollable area
            float width = 450f;
            float height = Screen.height - 20f;
            int lineCount = text.Split('\n').Length;
            float contentHeight = lineCount * 20f + 40f;
            
            _scrollPosition = GUI.BeginScrollView(new Rect(10, 10, width, height), _scrollPosition, 
                new Rect(0, 0, width - 20, contentHeight));
            
            GUI.Box(new Rect(0, 0, width - 20, contentHeight), text, style);
            
            GUI.EndScrollView();
        }
        
        private string DisplayRockStateDetails(TutorialScriptable scriptable)
        {
            string text = "";
            
            // Rock State Set from Scriptable
            if (scriptable != null && _rockStateSetProp != null)
            {
                var rockStateSet = _rockStateSetProp.GetValue(scriptable);
                if (rockStateSet != null)
                {
                    text += "\n<color=#FFD700><b>[Rock State Set (Config)]</b></color>\n";
                    text += GetStructFieldsString(rockStateSet, rockStateSet.GetType());
                }
            }
            
            // Rock State Deps
            if (_rockStateDepsField != null)
            {
                var rockStateDeps = _rockStateDepsField.GetValue(_tutorialStateManager);
                if (rockStateDeps != null)
                {
                    text += "\n<color=#00CED1><b>[Rock State Deps]</b></color>\n";
                    text += GetStructFieldsString(rockStateDeps, rockStateDeps.GetType());
                }
            }
            
            // Rock component
            if (_rock != null)
            {
                text += "\n<color=#00FFFF><b>[Rock Component]</b></color>\n";
                
                if (_isShakingField != null)
                {
                    bool isShaking = (bool)_isShakingField.GetValue(_rock);
                    text += $"Is Shaking: {(isShaking ? "<color=yellow><b>YES</b></color>" : "<color=gray>NO</color>")}\n";
                }
                
                if (_accumulatedShakeTimeField != null)
                {
                    float accumulatedTime = (float)_accumulatedShakeTimeField.GetValue(_rock);
                    text += $"Accumulated Shake Time: <b>{accumulatedTime:F2}s</b>\n";
                }
                
                if (_eventTriggeredField != null)
                {
                    bool eventTriggered = (bool)_eventTriggeredField.GetValue(_rock);
                    text += $"Event Triggered: {(eventTriggered ? "<color=green><b>YES</b></color>" : "<color=gray>NO</color>")}\n";
                }
                
                if (_currentShakeTweenField != null)
                {
                    var tween = _currentShakeTweenField.GetValue(_rock) as Tween;
                    text += $"Shake Tween: {(tween != null && tween.IsActive() ? "<color=green>Active</color>" : "<color=gray>Inactive</color>")}\n";
                }
                
                if (_rockShakeSettingsField != null)
                {
                    var shakeSettings = _rockShakeSettingsField.GetValue(_rock);
                    if (shakeSettings != null)
                    {
                        text += "\n<color=#FFD700><b>[Rock Shake Settings (Runtime)]</b></color>\n";
                        text += GetStructFieldsString(shakeSettings, shakeSettings.GetType());
                    }
                }
            }
            else
            {
                text += "\n<color=#00FFFF><b>[Rock Component]</b></color>\n";
                text += "<i>Rock not found...</i>\n";
            }
            
            return text;
        }
        
        private string DisplayRiseToBossStateDetails(TutorialScriptable scriptable)
        {
            string text = "";
            
            // RiseToBoss State Set from Scriptable
            if (scriptable != null && _riseToBossStateSetProp != null)
            {
                var riseToBossStateSet = _riseToBossStateSetProp.GetValue(scriptable);
                if (riseToBossStateSet != null)
                {
                    text += "\n<color=#FFD700><b>[RiseToBoss State Set (Config)]</b></color>\n";
                    text += GetStructFieldsString(riseToBossStateSet, riseToBossStateSet.GetType());
                }
            }
            
            // RiseToBoss State Deps
            if (_riseToBossStateDepsField != null)
            {
                var riseToBossStateDeps = _riseToBossStateDepsField.GetValue(_tutorialStateManager);
                if (riseToBossStateDeps != null)
                {
                    text += "\n<color=#00CED1><b>[RiseToBoss State Deps]</b></color>\n";
                    text += GetStructFieldsString(riseToBossStateDeps, riseToBossStateDeps.GetType());
                }
            }
            
            return text;
        }
        
        private string DisplaySpellHitStateDetails(TutorialScriptable scriptable)
        {
            string text = "";
            
            // SpellHit State Set from Scriptable
            if (scriptable != null && _spellHitStateSetProp != null)
            {
                var spellHitStateSet = _spellHitStateSetProp.GetValue(scriptable);
                if (spellHitStateSet != null)
                {
                    text += "\n<color=#FFD700><b>[SpellHit State Set (Config)]</b></color>\n";
                    text += GetStructFieldsString(spellHitStateSet, spellHitStateSet.GetType());
                }
            }
            
            // SpellHit State Deps
            if (_spellHitStateDepsField != null)
            {
                var spellHitStateDeps = _spellHitStateDepsField.GetValue(_tutorialStateManager);
                if (spellHitStateDeps != null)
                {
                    text += "\n<color=#00CED1><b>[SpellHit State Deps]</b></color>\n";
                    text += GetStructFieldsString(spellHitStateDeps, spellHitStateDeps.GetType());
                    
                    // Show slime positions in real-time
                    var spellHitStateDepsType = spellHitStateDeps.GetType();
                    var slime1Field = spellHitStateDepsType.GetField("slime1");
                    var slime2Field = spellHitStateDepsType.GetField("slime2");
                    
                    if (slime1Field != null && slime2Field != null)
                    {
                        Transform slime1 = slime1Field.GetValue(spellHitStateDeps) as Transform;
                        Transform slime2 = slime2Field.GetValue(spellHitStateDeps) as Transform;
                        
                        if (slime1 != null && slime2 != null)
                        {
                            text += "\n<color=#90EE90><b>[Real-Time Slime Positions]</b></color>\n";
                            text += $"Slime1 X: <b>{slime1.position.x:F2}</b>\n";
                            text += $"Slime2 X: <b>{slime2.position.x:F2}</b>\n";
                            
                            string leftSlime = slime1.position.x < slime2.position.x ? "Slime1" : "Slime2";
                            text += $"Left Slime (X < means left): <color=yellow><b>{leftSlime}</b></color>\n";
                            text += $"Current Logic Target: <color=yellow><b>{leftSlime}</b></color>\n";
                        }
                    }
                }
            }
            
            return text;
        }
    }
}