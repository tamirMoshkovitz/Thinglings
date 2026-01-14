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
        
        // Reflection info for RiseToBossLogic runtime tracking
        private FieldInfo _allLogicsField;
        private float _riseToBossLastCameraY;
        private float _riseToBossTimeSinceMovement;
        private bool _riseToBossTrackingStarted;
        private TutorialState _lastTrackedState;
        
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
            _allLogicsField = t.GetField("_allLogics", BindingFlags.Instance | BindingFlags.NonPublic);
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
            
            // Reset tracking when state changes
            if (currentState != _lastTrackedState)
            {
                _riseToBossTrackingStarted = false;
                _riseToBossTimeSinceMovement = 0f;
                _lastTrackedState = currentState;
            }
            
            text += "\n<color=#FFA500><b>[Tutorial State Manager]</b></color>\n";
            text += $"Current State: <color=yellow><b>{currentState}</b></color>\n";
            text += $"Frame: <b>{Time.frameCount}</b> | Time: <b>{Time.time:F1}s</b> | DeltaTime: <b>{Time.deltaTime * 1000:F1}ms</b>\n";
            
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
            object riseToBossStateSet = null;
            if (scriptable != null && _riseToBossStateSetProp != null)
            {
                riseToBossStateSet = _riseToBossStateSetProp.GetValue(scriptable);
                if (riseToBossStateSet != null)
                {
                    text += "\n<color=#FFD700><b>[RiseToBoss State Set (Config)]</b></color>\n";
                    text += GetStructFieldsString(riseToBossStateSet, riseToBossStateSet.GetType());
                }
            }
            
            // RiseToBoss State Deps
            object riseToBossStateDeps = null;
            if (_riseToBossStateDepsField != null)
            {
                riseToBossStateDeps = _riseToBossStateDepsField.GetValue(_tutorialStateManager);
                if (riseToBossStateDeps != null)
                {
                    text += "\n<color=#00CED1><b>[RiseToBoss State Deps]</b></color>\n";
                    text += GetStructFieldsString(riseToBossStateDeps, riseToBossStateDeps.GetType());
                }
            }
            
            // Real-time tracking and calculations
            if (riseToBossStateDeps != null && riseToBossStateSet != null)
            {
                text += DisplayRiseToBossRealTimeData(riseToBossStateDeps, riseToBossStateSet);
            }
            
            // Get RiseToBossLogic from _allLogics and show tween state
            if (_allLogicsField != null)
            {
                text += DisplayRiseToBossLogicState();
            }
            
            return text;
        }
        
        private string DisplayRiseToBossRealTimeData(object riseToBossStateDeps, object riseToBossStateSet)
        {
            string text = "";
            Type depsType = riseToBossStateDeps.GetType();
            Type setType = riseToBossStateSet.GetType();
            
            // Get deps fields
            var slime1Field = depsType.GetField("Slime1");
            var slime2Field = depsType.GetField("Slime2");
            var arrow1Field = depsType.GetField("Arrow1");
            var arrow2Field = depsType.GetField("Arrow2");
            var mainCameraField = depsType.GetField("mainCamera");
            var maxCameraPositionField = depsType.GetField("maxCameraPosition");
            
            // Get settings fields
            var topThresholdPercentField = setType.GetField("topThresholdPercent");
            var cameraNotMovingTimeoutField = setType.GetField("cameraNotMovingTimeout");
            
            GameObject slime1 = slime1Field?.GetValue(riseToBossStateDeps) as GameObject;
            GameObject slime2 = slime2Field?.GetValue(riseToBossStateDeps) as GameObject;
            Animator arrow1 = arrow1Field?.GetValue(riseToBossStateDeps) as Animator;
            Animator arrow2 = arrow2Field?.GetValue(riseToBossStateDeps) as Animator;
            Camera mainCamera = mainCameraField?.GetValue(riseToBossStateDeps) as Camera;
            Transform maxCameraPosition = maxCameraPositionField?.GetValue(riseToBossStateDeps) as Transform;
            
            int topThresholdPercent = topThresholdPercentField != null ? (int)topThresholdPercentField.GetValue(riseToBossStateSet) : 0;
            float cameraNotMovingTimeout = cameraNotMovingTimeoutField != null ? (float)cameraNotMovingTimeoutField.GetValue(riseToBossStateSet) : 0f;
            
            if (mainCamera == null) return text;
            
            text += "\n<color=#90EE90><b>[Real-Time Camera Data]</b></color>\n";
            
            float currentCameraY = mainCamera.transform.position.y;
            float orthographicSize = mainCamera.orthographicSize;
            
            // Track camera movement (simulating the coroutine logic)
            if (!_riseToBossTrackingStarted)
            {
                _riseToBossLastCameraY = currentCameraY;
                _riseToBossTimeSinceMovement = 0f;
                _riseToBossTrackingStarted = true;
            }
            
            float cameraMovementDelta = Mathf.Abs(currentCameraY - _riseToBossLastCameraY);
            
            if (cameraMovementDelta > 0.001f)
            {
                _riseToBossTimeSinceMovement = 0f;
                _riseToBossLastCameraY = currentCameraY;
            }
            else
            {
                _riseToBossTimeSinceMovement += Time.deltaTime;
            }
            
            text += $"Camera Y: <b>{currentCameraY:F3}</b>\n";
            text += $"Last Camera Y: <b>{_riseToBossLastCameraY:F3}</b>\n";
            text += $"Camera Movement Delta: <b>{cameraMovementDelta:F5}</b> (threshold: 0.001)\n";
            text += $"Orthographic Size: <b>{orthographicSize:F2}</b>\n";
            
            // Movement timeout tracking
            bool shouldShowArrows = _riseToBossTimeSinceMovement >= cameraNotMovingTimeout;
            string timeColor = shouldShowArrows ? "red" : "white";
            text += $"Time Since Movement: <color={timeColor}><b>{_riseToBossTimeSinceMovement:F2}s</b></color> / {cameraNotMovingTimeout:F1}s\n";
            text += $"Should Show Arrows: {(shouldShowArrows ? "<color=yellow><b>YES</b></color>" : "<color=gray>NO</color>")}\n";
            
            if (maxCameraPosition != null)
            {
                float maxY = maxCameraPosition.position.y;
                float distanceToMax = maxY - currentCameraY;
                text += $"Max Camera Y: <b>{maxY:F2}</b>\n";
                text += $"Distance to Max: <b>{distanceToMax:F2}</b>\n";
            }
            
            // Threshold calculation
            text += "\n<color=#FF69B4><b>[Slimes At Top Check]</b></color>\n";
            float cameraCenter = currentCameraY;
            float thresholdPercentage = topThresholdPercent / 100f;
            float threshold = cameraCenter + orthographicSize * (1f - thresholdPercentage);
            
            text += $"Threshold Calculation:\n";
            text += $"  Center({cameraCenter:F2}) + Size({orthographicSize:F2}) * (1 - {thresholdPercentage:F2})\n";
            text += $"  = <b>{threshold:F2}</b>\n";
            
            if (slime1 != null && slime2 != null)
            {
                float slime1Y = slime1.transform.position.y;
                float slime2Y = slime2.transform.position.y;
                
                bool slime1AtTop = slime1Y >= threshold;
                bool slime2AtTop = slime2Y >= threshold;
                
                float slime1Diff = slime1Y - threshold;
                float slime2Diff = slime2Y - threshold;
                
                string s1Color = slime1AtTop ? "green" : "red";
                string s2Color = slime2AtTop ? "green" : "red";
                
                text += $"\nSlime1 Y: <b>{slime1Y:F2}</b> (<color={s1Color}>{(slime1AtTop ? "AT TOP" : $"needs +{-slime1Diff:F2}")}</color>)\n";
                text += $"Slime2 Y: <b>{slime2Y:F2}</b> (<color={s2Color}>{(slime2AtTop ? "AT TOP" : $"needs +{-slime2Diff:F2}")}</color>)\n";
                text += $"Both At Top: {(slime1AtTop && slime2AtTop ? "<color=green><b>YES - Will proceed!</b></color>" : "<color=red><b>NO</b></color>")}\n";
            }
            
            // Arrow Animator states
            text += "\n<color=#00BFFF><b>[Arrow Animators]</b></color>\n";
            if (arrow1 != null)
            {
                var stateInfo1 = arrow1.GetCurrentAnimatorStateInfo(0);
                text += $"Arrow1: ";
                if (stateInfo1.IsName("arrow in"))
                    text += "<color=yellow><b>arrow in</b></color>\n";
                else if (stateInfo1.IsName("arrow out"))
                    text += "<color=gray>arrow out</color>\n";
                else
                    text += $"<color=orange>{stateInfo1.fullPathHash}</color>\n";
                text += $"  Normalized Time: <b>{stateInfo1.normalizedTime:F2}</b>\n";
            }
            else
            {
                text += "Arrow1: <color=red>NULL</color>\n";
            }
            
            if (arrow2 != null)
            {
                var stateInfo2 = arrow2.GetCurrentAnimatorStateInfo(0);
                text += $"Arrow2: ";
                if (stateInfo2.IsName("arrow in"))
                    text += "<color=yellow><b>arrow in</b></color>\n";
                else if (stateInfo2.IsName("arrow out"))
                    text += "<color=gray>arrow out</color>\n";
                else
                    text += $"<color=orange>{stateInfo2.fullPathHash}</color>\n";
                text += $"  Normalized Time: <b>{stateInfo2.normalizedTime:F2}</b>\n";
            }
            else
            {
                text += "Arrow2: <color=red>NULL</color>\n";
            }
            
            return text;
        }
        
        private string DisplayRiseToBossLogicState()
        {
            string text = "";
            
            try
            {
                var allLogics = _allLogicsField.GetValue(_tutorialStateManager) as System.Collections.IList;
                if (allLogics == null || allLogics.Count == 0)
                {
                    text += "\n<color=#FF6347><b>[RiseToBoss Logic]</b></color>\n";
                    text += "<i>No logic instances found</i>\n";
                    return text;
                }
                
                // Find RiseToBossLogic in the list
                object riseToBossLogic = null;
                foreach (var logic in allLogics)
                {
                    if (logic != null && logic.GetType().Name == "RiseToBossLogic")
                    {
                        riseToBossLogic = logic;
                        break;
                    }
                }
                
                if (riseToBossLogic != null)
                {
                    text += "\n<color=#FF6347><b>[RiseToBoss Logic Instance]</b></color>\n";
                    text += "<color=green>Logic found in _allLogics</color>\n";
                    
                    Type logicType = riseToBossLogic.GetType();
                    
                    // Get the camera tween field
                    var cameraTweenField = logicType.GetField("_cameraMoveTween", BindingFlags.Instance | BindingFlags.NonPublic);
                    if (cameraTweenField != null)
                    {
                        var tween = cameraTweenField.GetValue(riseToBossLogic) as Tween;
                        if (tween != null)
                        {
                            text += $"Camera Tween: <color=green><b>EXISTS</b></color>\n";
                            text += $"  IsActive: {(tween.IsActive() ? "<color=green>YES</color>" : "<color=gray>NO</color>")}\n";
                            text += $"  IsPlaying: {(tween.IsPlaying() ? "<color=yellow><b>YES</b></color>" : "<color=gray>NO</color>")}\n";
                            text += $"  IsComplete: {(tween.IsComplete() ? "<color=green>YES</color>" : "<color=gray>NO</color>")}\n";
                            if (tween.IsActive())
                            {
                                text += $"  Elapsed: <b>{tween.Elapsed():F2}s</b> / {tween.Duration():F2}s\n";
                                text += $"  Progress: <b>{tween.ElapsedPercentage() * 100:F1}%</b>\n";
                            }
                        }
                        else
                        {
                            text += $"Camera Tween: <color=gray>NULL (not started yet)</color>\n";
                        }
                    }
                    
                    // Get the deps and settings from the logic instance
                    var depsField = logicType.GetField("_riseToBossStateDeps", BindingFlags.Instance | BindingFlags.NonPublic);
                    var setField = logicType.GetField("_riseToBossStateSet", BindingFlags.Instance | BindingFlags.NonPublic);
                    
                    if (depsField != null)
                    {
                        var deps = depsField.GetValue(riseToBossLogic);
                        text += $"Logic has Deps: <color=green>YES</color>\n";
                    }
                    if (setField != null)
                    {
                        var set = setField.GetValue(riseToBossLogic);
                        text += $"Logic has Settings: <color=green>YES</color>\n";
                    }
                }
                else
                {
                    text += "\n<color=#FF6347><b>[RiseToBoss Logic Instance]</b></color>\n";
                    text += $"<i>RiseToBossLogic not found in {allLogics.Count} logics</i>\n";
                    
                    // List what logics exist
                    text += "Existing logics:\n";
                    foreach (var logic in allLogics)
                    {
                        if (logic != null)
                            text += $"  - {logic.GetType().Name}\n";
                    }
                }
            }
            catch (Exception e)
            {
                text += $"\n<color=red>Error accessing logic: {e.Message}</color>\n";
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