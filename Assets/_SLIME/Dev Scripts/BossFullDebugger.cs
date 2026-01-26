using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using _SLIME.Boss;
using Object = UnityEngine.Object;

namespace _SLIME.DevScripts
{
    /// <summary>
    /// Full Boss debugger – attach to any GameObject. Uses Reflection only, no SerializedField.
    /// Finds BossBrain in scene at runtime and displays all relevant data.
    /// </summary>
    public class BossFullDebugger : MonoBehaviour
    {
        private BossBrain _brain;
        private PropertyInfo _stateMachineProp;
        private Vector2 _scroll;
        private bool _foldoutBrain = true;
        private bool _foldoutConfig = true;
        private bool _foldoutState = true;
        private string _lastError;
        private float _refreshTimer;

        private const float RefreshInterval = 0.5f;

        private void Update()
        {
            _refreshTimer += Time.deltaTime;
            if (_brain == null || _refreshTimer >= RefreshInterval)
            {
                _refreshTimer = 0f;
                ResolveBossBrain();
            }
        }

        private void ResolveBossBrain()
        {
            _lastError = null;
            _brain = Object.FindFirstObjectByType<BossBrain>();
            if (_brain == null)
            {
                _lastError = "BossBrain not found in scene.";
                return;
            }

            if (_stateMachineProp == null)
            {
                _stateMachineProp = typeof(BossBrain).GetProperty("StateMachine",
                    BindingFlags.NonPublic | BindingFlags.Instance);
            }
        }

        private void OnGUI()
        {
            float w = 420f;
            float h = Screen.height - 20f;
            GUILayout.BeginArea(new Rect(Screen.width - w - 10f, 10f, w, h));
            _scroll = GUILayout.BeginScrollView(_scroll, "box");

            var titleStyle = new GUIStyle(GUI.skin.label) { richText = true, fontSize = 15 };
            GUILayout.Label("<b>Boss Full Debugger</b> (Reflection, no refs)", titleStyle);
            GUILayout.Space(4f);

            if (!string.IsNullOrEmpty(_lastError))
            {
                GUILayout.Label($"<color=red>{_lastError}</color>", titleStyle);
                GUILayout.EndScrollView();
                GUILayout.EndArea();
                return;
            }

            if (_brain == null)
            {
                GUILayout.Label("<color=yellow>Resolving BossBrain...</color>", titleStyle);
                GUILayout.EndScrollView();
                GUILayout.EndArea();
                return;
            }

            DrawStaticSection(titleStyle);
            DrawBrainSection(titleStyle);
            DrawStateMachineSection(titleStyle);
            DrawConfigSection(titleStyle);

            GUILayout.EndScrollView();
            GUILayout.EndArea();
        }

        private void DrawStaticSection(GUIStyle titleStyle)
        {
            _foldoutState = GUILayout.Toggle(_foldoutState, (_foldoutState ? "▼ " : "▶ ") + "State & static", "button");
            if (!_foldoutState) return;

            GUILayout.Space(4f);
            GUILayout.Label($"<b>BossState</b>: {BossBrain.BossState}");
            var config = BossBrain.bossConfigurations;
            GUILayout.Label($"<b>bossConfigurations</b>: {(config != null ? config.name : "null")}");
            GUILayout.Space(6f);
        }

        private void DrawBrainSection(GUIStyle titleStyle)
        {
            _foldoutBrain = GUILayout.Toggle(_foldoutBrain, (_foldoutBrain ? "▼ " : "▶ ") + "BossBrain instance", "button");
            if (!_foldoutBrain) return;

            GUILayout.Space(4f);
            var t = _brain.GetType();
            DrawFields(t, _brain, BindingFlags.Public | BindingFlags.Instance);
            DrawFields(t, _brain, BindingFlags.NonPublic | BindingFlags.Instance);
            DrawProperties(t, _brain, BindingFlags.Public | BindingFlags.Instance);
            GUILayout.Space(6f);
        }

        private void DrawStateMachineSection(GUIStyle titleStyle)
        {
            if (_stateMachineProp == null) return;

            object sm = _stateMachineProp.GetValue(_brain);
            if (sm == null) return;

            var smType = sm.GetType();
            var currentStateProp = smType.GetProperty("CurrentState", BindingFlags.Public | BindingFlags.Instance);
            if (currentStateProp == null) return;

            object state = currentStateProp.GetValue(sm);
            if (state == null) return;

            string stateName = state.GetType().Name;
            string enterHealth = "-";
            var eh = state.GetType().GetField("EnterHealth", BindingFlags.Public | BindingFlags.Instance);
            if (eh != null)
            {
                var v = eh.GetValue(state);
                enterHealth = v != null ? v.ToString() : "null";
            }

            GUILayout.Label("<b>StateMachine</b>", titleStyle);
            GUILayout.Space(2f);
            GUILayout.Label($"CurrentState: <b>{stateName}</b>");
            GUILayout.Label($"EnterHealth: {enterHealth}");
            GUILayout.Space(6f);
        }

        private void DrawConfigSection(GUIStyle titleStyle)
        {
            var config = BossBrain.bossConfigurations;
            if (config == null) return;

            _foldoutConfig = GUILayout.Toggle(_foldoutConfig, (_foldoutConfig ? "▼ " : "▶ ") + "BaseBossConfigurations", "button");
            if (!_foldoutConfig) return;

            GUILayout.Space(4f);
            var ct = config.GetType();
            foreach (var prop in ct.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                if (prop.GetIndexParameters().Length > 0) continue;
                try
                {
                    object val = prop.GetValue(config);
                    string valStr = FormatValue(val);
                    var vt = val?.GetType();
                    bool isStruct = vt != null && vt.IsValueType && !vt.IsPrimitive && vt != typeof(string) &&
                                    !(val is Vector2) && !(val is Vector3);
                    if (isStruct)
                    {
                        GUILayout.Label($"<b>{prop.Name}</b>:");
                        DrawStructFields(val, "  ");
                        GUILayout.Space(2f);
                    }
                    else
                    {
                        GUILayout.Label($"<b>{prop.Name}</b>: {valStr}");
                    }
                }
                catch (Exception ex)
                {
                    GUILayout.Label($"<b>{prop.Name}</b>: <color=red>{ex.Message}</color>");
                }
            }
            GUILayout.Space(6f);
        }

        private void DrawStructFields(object obj, string prefix = "  ", int maxDepth = 5)
        {
            if (obj == null || maxDepth <= 0) return;
            var st = obj.GetType();
            foreach (var f in st.GetFields(BindingFlags.Public | BindingFlags.Instance))
            {
                try
                {
                    object v = f.GetValue(obj);
                    var vt = v?.GetType();
                    bool nestedStruct = vt != null && vt.IsValueType && !vt.IsPrimitive && vt != typeof(string) &&
                                        !(v is Vector2) && !(v is Vector3);
                    if (nestedStruct)
                    {
                        GUILayout.Label($"{prefix}{f.Name}:");
                        DrawStructFields(v, prefix + "  ", maxDepth - 1);
                    }
                    else
                    {
                        GUILayout.Label($"{prefix}{f.Name}: {FormatValue(v)}");
                    }
                }
                catch (Exception ex)
                {
                    GUILayout.Label($"{prefix}{f.Name}: <color=red>{ex.Message}</color>");
                }
            }
        }

        private void DrawFields(Type t, object target, BindingFlags flags)
        {
            if (target == null) return;
            var fields = t.GetFields(flags);
            foreach (var f in fields)
            {
                if (f.IsStatic) continue;
                if (f.Name.Contains("k__BackingField")) continue;
                try
                {
                    object v = f.GetValue(target);
                    GUILayout.Label($"<b>{f.Name}</b>: {FormatValue(v)}");
                }
                catch (Exception ex)
                {
                    GUILayout.Label($"<b>{f.Name}</b>: <color=red>{ex.Message}</color>");
                }
            }
        }

        private void DrawProperties(Type t, object target, BindingFlags flags)
        {
            if (target == null) return;
            foreach (var p in t.GetProperties(flags))
            {
                if (p.GetIndexParameters().Length > 0) continue;
                try
                {
                    object v = p.GetValue(target);
                    GUILayout.Label($"<b>{p.Name}</b>: {FormatValue(v)}");
                }
                catch (Exception ex)
                {
                    GUILayout.Label($"<b>{p.Name}</b>: <color=red>{ex.Message}</color>");
                }
            }
        }

        private static string FormatValue(object v)
        {
            if (v == null) return "null";
            if (v is Object uo) return uo ? uo.name : "null";
            if (v is IList list) return $"[{list.Count}]";
            if (v is ICollection col) return $"[{col.Count}]";
            if (v is Vector2 v2) return $"{v2.x:F2}, {v2.y:F2}";
            if (v is Vector3 v3) return $"{v3.x:F2}, {v3.y:F2}, {v3.z:F2}";
            if (v is float f) return f.ToString("F2");
            return v.ToString();
        }
    }
}
