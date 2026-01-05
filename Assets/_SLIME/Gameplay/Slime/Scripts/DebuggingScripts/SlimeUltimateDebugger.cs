using UnityEngine;
using System.Reflection;
using System;
using _SLIME.Slime; // מוודא שה-Namespace קיים

public class SlimeUltimateDebugger : MonoBehaviour
{
    // --- Instances we found in the scene ---
    private object _dataInstance;       // המופע של SlimeData
    private object _connectionsInstance; // המופע של SlimeConnections
    private object _configInstance;     // המופע של SlimeConfiguration
    
    // --- Reflection Info for SlimeData ---
    private PropertyInfo _connectedProp;
    private PropertyInfo _distanceProp;
    private PropertyInfo _maxStretchProp;
    private PropertyInfo _stretchRatioProp;
    private PropertyInfo _isStrainedProp;
    private PropertyInfo _jointsCountProp; // זה ה-SpringJointCount (פיזיקה)
    
    // --- Reflection Info for SlimeConnections ---
    private FieldInfo _numConnectionsField; // זה ה-_numOfSlimeConnections (לוגיקה)
    private FieldInfo _currentStretchTimerField;
    private FieldInfo _shouldTearAllConnectionsField;
    
    // --- Reflection Info for SlimeConfiguration ---
    private PropertyInfo _breakForceProp;
    private PropertyInfo _connectionFrequencyProp;
    private PropertyInfo _connectionDampingRatioProp;
    private FieldInfo _maxConnectionsField;
    private PropertyInfo _maxStretchTimeThresholdProp;
    private PropertyInfo _maxStretchPercentThresholdProp;

    // --- For Gizmos ---
    private FieldInfo _sideAField;
    private FieldInfo _sideBField;

    private void Start()
    {
        FindInstancesInScene();
    }

    private void Update()
    {
        // רענון אם לא מצאנו או כל 100 פריימים כדי לוודא שדברים לא השתנו
        if ((_dataInstance == null || _connectionsInstance == null || _configInstance == null) && Time.frameCount % 100 == 0)
        {
            FindInstancesInScene();
        }
    }

    private void FindInstancesInScene()
    {
        // עובר על כל ה-MonoBehaviours בסצנה
        foreach (var mb in FindObjectsOfType<MonoBehaviour>())
        {
            if (mb == this) continue;

            // משיג את כל השדות של הסקריפט הנוכחי
            var fields = mb.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            foreach (var field in fields)
            {
                // 1. חיפוש SlimeData
                if (field.FieldType == typeof(SlimeData))
                {
                    var val = field.GetValue(mb);
                    if (val != null)
                    {
                        _dataInstance = val;
                        InitDataReflection();
                    }
                }
                
                // 2. חיפוש SlimeConnections
                if (field.FieldType == typeof(SlimeConnections))
                {
                    var val = field.GetValue(mb);
                    if (val != null)
                    {
                        _connectionsInstance = val;
                        InitConnectionsReflection();
                    }
                }
                
                // 3. חיפוש SlimeConfiguration
                if (field.FieldType == typeof(SlimeConfiguration))
                {
                    var val = field.GetValue(mb);
                    if (val != null)
                    {
                        _configInstance = val;
                        InitConfigReflection();
                    }
                }
            }
        }
    }

    private void InitDataReflection()
    {
        Type t = _dataInstance.GetType();
        _connectedProp = t.GetProperty("Connected");
        _distanceProp = t.GetProperty("Distance");
        _maxStretchProp = t.GetProperty("MaxStretchDistance");
        _stretchRatioProp = t.GetProperty("StretchRatio");
        _isStrainedProp = t.GetProperty("IsStrained");
        _jointsCountProp = t.GetProperty("SpringJointCount");
        
        _sideAField = t.GetField("_sideA", BindingFlags.Instance | BindingFlags.NonPublic);
        _sideBField = t.GetField("_sideB", BindingFlags.Instance | BindingFlags.NonPublic);
    }

    private void InitConnectionsReflection()
    {
        Type t = _connectionsInstance.GetType();
        // שליפת השדה הפרטי _numOfSlimeConnections
        _numConnectionsField = t.GetField("_numOfSlimeConnections", BindingFlags.Instance | BindingFlags.NonPublic);
        _currentStretchTimerField = t.GetField("_currentStretchTimer", BindingFlags.Instance | BindingFlags.NonPublic);
        _shouldTearAllConnectionsField = t.GetField("_shouldTearAllConnections", BindingFlags.Instance | BindingFlags.NonPublic);
    }

    private void InitConfigReflection()
    {
        Type t = _configInstance.GetType();
        _breakForceProp = t.GetProperty("BreakForce");
        _connectionFrequencyProp = t.GetProperty("ConnectionFrequency");
        _connectionDampingRatioProp = t.GetProperty("ConnectionDampingRatio");
        _maxConnectionsField = t.GetField("MaxConnectionsOfSlime", BindingFlags.Instance | BindingFlags.Public);
        _maxStretchTimeThresholdProp = t.GetProperty("MaxStretchTimeThreshold");
        _maxStretchPercentThresholdProp = t.GetProperty("MaxStretchPercentThreshold");
    }

    private void OnGUI()
    {
        if (_dataInstance == null && _connectionsInstance == null && _configInstance == null)
        {
            GUI.Label(new Rect(10, 10, 300, 20), "Scanning scene for Slime classes...");
            return;
        }

        GUIStyle style = new GUIStyle(GUI.skin.box);
        style.alignment = TextAnchor.UpperLeft;
        style.fontSize = 13;
        style.normal.textColor = new Color(0.9f, 0.9f, 0.9f);
        style.richText = true;

        string text = "<b><size=15>--- SLIME DEBUGGER ---</size></b>\n";

        // --- SECTION 1: SLIME CONNECTIONS (LOGIC) ---
        text += "\n<color=#FFA500><b>[Logic Connections]</b></color>\n";
        if (_connectionsInstance != null && _numConnectionsField != null)
        {
            int realConnections = (int)_numConnectionsField.GetValue(_connectionsInstance);
            text += $"_numOfSlimeConnections: <b>{realConnections}</b>\n";
        }
        else
        {
            text += "<i>SlimeConnections not found yet...</i>\n";
        }
        
        // --- SECTION 1.5: STRETCH TIME THRESHOLD ---
        text += "\n<color=#FFD700><b>[Stretch Time Threshold]</b></color>\n";
        if (_configInstance != null)
        {
            if (_maxStretchTimeThresholdProp != null && _maxStretchPercentThresholdProp != null)
            {
                float timeThreshold = (float)_maxStretchTimeThresholdProp.GetValue(_configInstance);
                float percentThreshold = (float)_maxStretchPercentThresholdProp.GetValue(_configInstance);
                text += $"Time Threshold: <b>{timeThreshold:F2}s</b>\n";
                text += $"Percent Threshold: <b>{percentThreshold:F1}%</b>\n";
            }
        }
        
        if (_connectionsInstance != null)
        {
            if (_currentStretchTimerField != null)
            {
                float currentTimer = (float)_currentStretchTimerField.GetValue(_connectionsInstance);
                if (currentTimer > 0f)
                {
                    text += $"Current Timer: <color=yellow><b>{currentTimer:F2}s</b></color>\n";
                }
                else
                {
                    text += $"Current Timer: <color=gray>0.00s</color>\n";
                }
            }
            
            if (_shouldTearAllConnectionsField != null)
            {
                bool shouldTear = (bool)_shouldTearAllConnectionsField.GetValue(_connectionsInstance);
                text += $"Should Tear All: {(shouldTear ? "<color=red><b>YES</b></color>" : "<color=green>NO</color>")}";
            }
        }
        else
        {
            text += "<i>SlimeConnections not found yet...</i>";
        }

        // --- SECTION 2: SLIME DATA (PHYSICS) ---
        text += "\n<color=#00FFFF><b>[Physics Data]</b></color>\n";
        if (_dataInstance != null)
        {
            bool connected = (bool)_connectedProp.GetValue(_dataInstance);
            float distance = (float)_distanceProp.GetValue(_dataInstance);
            float maxStretch = (float)_maxStretchProp.GetValue(_dataInstance);
            float ratio = (float)_stretchRatioProp.GetValue(_dataInstance);
            bool strained = (bool)_isStrainedProp.GetValue(_dataInstance);
            int springs = (int)_jointsCountProp.GetValue(_dataInstance);

            text += $"Connected: {(connected ? "<color=green>TRUE</color>" : "<color=red>FALSE</color>")}\n" +
                    $"SpringJoint Count: {springs}\n" +
                    $"Dist: {distance:F2} / Max: {maxStretch:F2}\n" +
                    $"Ratio: <b>{ratio:P0}</b>\n" +
                    $"Strained: {(strained ? "<color=yellow>YES</color>" : "NO")}";
        }
        else
        {
            text += "<i>SlimeData not found yet...</i>";
        }

        // --- SECTION 3: SLIME CONFIGURATION (STRETCH SETTINGS) ---
        text += "\n\n<color=#FF69B4><b>[Config - Stretch Settings]</b></color>\n";
        if (_configInstance != null)
        {
            if (_breakForceProp != null)
            {
                float breakForce = (float)_breakForceProp.GetValue(_configInstance);
                text += $"Break Force: <b>{breakForce:F1}</b>\n";
            }
            
            if (_connectionFrequencyProp != null)
            {
                float frequency = (float)_connectionFrequencyProp.GetValue(_configInstance);
                text += $"Connection Frequency: <b>{frequency:F2}</b>\n";
            }
            
            if (_connectionDampingRatioProp != null)
            {
                float damping = (float)_connectionDampingRatioProp.GetValue(_configInstance);
                text += $"Damping Ratio: <b>{damping:F2}</b>\n";
            }
            
            if (_maxConnectionsField != null)
            {
                int maxConnections = (int)_maxConnectionsField.GetValue(_configInstance);
                text += $"Max Connections: <b>{maxConnections}</b>";
            }
        }
        else
        {
            text += "<i>SlimeConfiguration not found yet...</i>";
        }

        // Draw Box
        float height = 450;
        GUI.Box(new Rect(20, 20, 260, height), text, style);
    }

    // --- Gizmos Logic (Same as before) ---
    private void OnDrawGizmos()
    {
        if (_dataInstance == null || _sideAField == null || _sideBField == null) return;

        object sideA = _sideAField.GetValue(_dataInstance);
        object sideB = _sideBField.GetValue(_dataInstance);
        if (sideA == null || sideB == null) return;

        Vector3 posA = GetPos(sideA);
        Vector3 posB = GetPos(sideB);

        float ratio = (float)_stretchRatioProp.GetValue(_dataInstance);
        
        Gizmos.color = ratio > 1f ? Color.red : Color.green;
        Gizmos.DrawLine(posA, posB);
        Gizmos.DrawSphere(posA, 0.3f);
        Gizmos.DrawSphere(posB, 0.3f);
    }

    private Vector3 GetPos(object side)
    {
        var prop = side.GetType().GetProperty("Position");
        return prop != null ? (Vector3)prop.GetValue(side) : Vector3.zero;
    }
}