using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using _SLIME.Boss;

namespace _SLIME.DevScripts
{
    public class BossHandsDebugger : MonoBehaviour
    {
        [Header("Gizmo Settings")]
        [SerializeField] private Color activeHandColor = Color.red;
        [SerializeField] private Color inactiveHandColor = Color.gray;
        [SerializeField] private Color warningColor = Color.yellow;
        [SerializeField] private float sphereRadius = 0.5f;

        // Cached instances
        private BossBrain _bossBrain;
        private BossHandsAttackBehaviour _behaviour;
        
        // Reflection fields for BossHandsAttackBehaviour
        private FieldInfo _smashRoutineField;
        private FieldInfo _leftHandsField;
        private FieldInfo _rightHandsField;
        
        // Reflection fields for HandWrapper (nested class)
        private System.Type _handWrapperType;
        private PropertyInfo _rootProperty;
        private PropertyInfo _logicProperty;
        
        // Reflection fields for BossHandAttackLogic
        private FieldInfo _splineAnimateField;
        private FieldInfo _warningVisualField;
        private PropertyInfo _isAttackingProperty;
        
        // GUI scroll
        private Vector2 _scrollPosition;
        
        private struct HandDebugInfo
        {
            public string name;
            public bool isActive;
            public bool isAttacking;
            public bool warningVisible;
            public float splineProgress;
            public Vector3 position;
        }
        
        private List<HandDebugInfo> _leftHandInfos = new List<HandDebugInfo>();
        private List<HandDebugInfo> _rightHandInfos = new List<HandDebugInfo>();
        private bool _behaviourActive;
        private bool _hasSmashRoutine;
        
        private void Start()
        {
            FindInstances();
            CacheReflectionFields();
        }
        
        private void FindInstances()
        {
            _bossBrain = FindFirstObjectByType<BossBrain>();
            
            if (_bossBrain != null && _bossBrain.animator != null)
            {
                var behaviours = _bossBrain.animator.GetBehaviours<BossHandsAttackBehaviour>();
                if (behaviours.Length > 0)
                {
                    _behaviour = behaviours[0];
                }
            }
        }
        
        private void CacheReflectionFields()
        {
            var behaviourType = typeof(BossHandsAttackBehaviour);
            
            _smashRoutineField = behaviourType.GetField("_smashRoutine", BindingFlags.NonPublic | BindingFlags.Instance);
            _leftHandsField = behaviourType.GetField("_leftHands", BindingFlags.NonPublic | BindingFlags.Instance);
            _rightHandsField = behaviourType.GetField("_rightHands", BindingFlags.NonPublic | BindingFlags.Instance);
            
            // Get the nested HandWrapper type
            _handWrapperType = behaviourType.GetNestedType("HandWrapper", BindingFlags.NonPublic);
            if (_handWrapperType != null)
            {
                _rootProperty = _handWrapperType.GetProperty("Root", BindingFlags.Public | BindingFlags.Instance);
                _logicProperty = _handWrapperType.GetProperty("Logic", BindingFlags.Public | BindingFlags.Instance);
            }
            
            // BossHandAttackLogic fields
            var logicType = typeof(BossHandAttackLogic);
            _splineAnimateField = logicType.GetField("splineAnimate", BindingFlags.NonPublic | BindingFlags.Instance);
            _warningVisualField = logicType.GetField("warningVisual", BindingFlags.NonPublic | BindingFlags.Instance);
            _isAttackingProperty = logicType.GetProperty("IsAttacking", BindingFlags.Public | BindingFlags.Instance);
        }
        
        private void Update()
        {
            if (_bossBrain == null || _behaviour == null)
            {
                if (Time.frameCount % 60 == 0)
                {
                    FindInstances();
                }
                return;
            }
            
            UpdateHandInfos();
        }
        
        private void UpdateHandInfos()
        {
            _leftHandInfos.Clear();
            _rightHandInfos.Clear();
            
            // Check if behaviour has active routine
            var routine = _smashRoutineField?.GetValue(_behaviour);
            _hasSmashRoutine = routine != null;
            
            // Get hand lists from behaviour
            var leftHands = _leftHandsField?.GetValue(_behaviour);
            var rightHands = _rightHandsField?.GetValue(_behaviour);
            
            if (leftHands != null)
            {
                CollectHandInfos(leftHands as System.Collections.IList, _leftHandInfos, "Left");
            }
            else if (_bossBrain.leftHandSplines != null)
            {
                // Fallback to boss brain's spline list
                foreach (var spline in _bossBrain.leftHandSplines)
                {
                    if (spline != null)
                    {
                        var info = GetHandInfoFromGameObject(spline, "Left");
                        _leftHandInfos.Add(info);
                    }
                }
            }
            
            if (rightHands != null)
            {
                CollectHandInfos(rightHands as System.Collections.IList, _rightHandInfos, "Right");
            }
            else if (_bossBrain.rightHandSplines != null)
            {
                // Fallback to boss brain's spline list
                foreach (var spline in _bossBrain.rightHandSplines)
                {
                    if (spline != null)
                    {
                        var info = GetHandInfoFromGameObject(spline, "Right");
                        _rightHandInfos.Add(info);
                    }
                }
            }
            
            _behaviourActive = _hasSmashRoutine;
        }
        
        private void CollectHandInfos(System.Collections.IList handWrappers, List<HandDebugInfo> targetList, string side)
        {
            if (handWrappers == null) return;
            
            int index = 0;
            foreach (var wrapper in handWrappers)
            {
                var info = new HandDebugInfo();
                info.name = $"{side} Hand {index}";
                
                var root = _rootProperty?.GetValue(wrapper) as GameObject;
                var logic = _logicProperty?.GetValue(wrapper) as BossHandAttackLogic;
                
                if (root != null)
                {
                    info.isActive = root.activeSelf;
                    info.position = root.transform.position;
                }
                
                if (logic != null)
                {
                    info.isAttacking = (bool)(_isAttackingProperty?.GetValue(logic) ?? false);
                    
                    var warningVisual = _warningVisualField?.GetValue(logic) as GameObject;
                    info.warningVisible = warningVisual != null && warningVisual.activeSelf;
                    
                    var splineAnimate = _splineAnimateField?.GetValue(logic);
                    if (splineAnimate != null)
                    {
                        var normalizedTimeProp = splineAnimate.GetType().GetProperty("NormalizedTime");
                        if (normalizedTimeProp != null)
                        {
                            info.splineProgress = (float)normalizedTimeProp.GetValue(splineAnimate);
                        }
                    }
                }
                
                targetList.Add(info);
                index++;
            }
        }
        
        private HandDebugInfo GetHandInfoFromGameObject(GameObject root, string side)
        {
            var info = new HandDebugInfo();
            info.name = $"{side}: {root.name}";
            info.isActive = root.activeSelf;
            info.position = root.transform.position;
            
            var logic = root.GetComponentInChildren<BossHandAttackLogic>();
            if (logic != null)
            {
                info.isAttacking = (bool)(_isAttackingProperty?.GetValue(logic) ?? false);
                
                var warningVisual = _warningVisualField?.GetValue(logic) as GameObject;
                info.warningVisible = warningVisual != null && warningVisual.activeSelf;
                
                var splineAnimate = _splineAnimateField?.GetValue(logic);
                if (splineAnimate != null)
                {
                    var normalizedTimeProp = splineAnimate.GetType().GetProperty("NormalizedTime");
                    if (normalizedTimeProp != null)
                    {
                        info.splineProgress = (float)normalizedTimeProp.GetValue(splineAnimate);
                    }
                }
            }
            
            return info;
        }
        
        private void OnGUI()
        {
            float width = 350;
            float height = Screen.height - 20f;
            
            GUILayout.BeginArea(new Rect(Screen.width - width - 10, 10, width, height));
            _scrollPosition = GUILayout.BeginScrollView(_scrollPosition, "box");
            
            var titleStyle = new GUIStyle(GUI.skin.label) { richText = true, fontSize = 14 };
            GUILayout.Label("<b>Boss Hands Debugger</b>", titleStyle);
            
            if (_bossBrain == null)
            {
                GUILayout.Label("<color=red>BossBrain not found!</color>", titleStyle);
                GUILayout.EndScrollView();
                GUILayout.EndArea();
                return;
            }
            
            // Attack state info
            GUILayout.Label($"<b>Attack State:</b> {(_behaviourActive ? "<color=green>Active</color>" : "<color=gray>Inactive</color>")}", titleStyle);
            GUILayout.Space(5);
            
            // Config info
            if (BossBrain.bossConfigurations != null)
            {
                var config = BossBrain.bossConfigurations.HandsAttack;
                GUILayout.Label("<b>Configuration:</b>", titleStyle);
                GUILayout.Label($"  Warning Duration: {config.handWarningDuration:F2}s");
                GUILayout.Label($"  Attack Duration: {config.handAttackDuration:F2}s");
                GUILayout.Label($"  Cooldown: {config.handCooldown:F2}s");
                GUILayout.Label($"  Total Hands: {config.totalHandsToUse}");
                GUILayout.Label($"  Use Both Hands: {config.useBothHands}");
            }
            
            GUILayout.Space(10);
            
            // Left hands
            GUILayout.Label($"<b><color=cyan>--- Left Hands ({_leftHandInfos.Count}) ---</color></b>", titleStyle);
            foreach (var info in _leftHandInfos)
            {
                DrawHandInfo(info);
            }
            
            GUILayout.Space(10);
            
            // Right hands
            GUILayout.Label($"<b><color=magenta>--- Right Hands ({_rightHandInfos.Count}) ---</color></b>", titleStyle);
            foreach (var info in _rightHandInfos)
            {
                DrawHandInfo(info);
            }
            
            GUILayout.EndScrollView();
            GUILayout.EndArea();
        }
        
        private void DrawHandInfo(HandDebugInfo info)
        {
            string activeStr = info.isActive ? "<color=green>ON</color>" : "<color=gray>OFF</color>";
            string attackingStr = info.isAttacking ? "<color=red>ATTACKING</color>" : "";
            string warningStr = info.warningVisible ? "<color=yellow>WARNING</color>" : "";
            
            var style = new GUIStyle(GUI.skin.label) { richText = true };
            GUILayout.Label($"<b>{info.name}</b> [{activeStr}] {attackingStr} {warningStr}", style);
            
            if (info.isActive)
            {
                GUILayout.Label($"  Progress: {info.splineProgress:P0}");
                GUILayout.Label($"  Position: {info.position}");
            }
        }
        
        private void OnDrawGizmos()
        {
            if (!Application.isPlaying) return;
            
            // Draw left hands
            foreach (var info in _leftHandInfos)
            {
                DrawHandGizmo(info, Color.cyan);
            }
            
            // Draw right hands
            foreach (var info in _rightHandInfos)
            {
                DrawHandGizmo(info, Color.magenta);
            }
        }
        
        private void DrawHandGizmo(HandDebugInfo info, Color baseColor)
        {
            if (!info.isActive)
            {
                Gizmos.color = inactiveHandColor;
            }
            else if (info.warningVisible)
            {
                Gizmos.color = warningColor;
            }
            else if (info.isAttacking)
            {
                Gizmos.color = activeHandColor;
            }
            else
            {
                Gizmos.color = baseColor;
            }
            
            Gizmos.DrawWireSphere(info.position, sphereRadius);
            
            // Draw progress bar
            if (info.isActive && info.splineProgress > 0)
            {
                Gizmos.color = Color.green;
                Vector3 progressEnd = info.position + Vector3.right * info.splineProgress * 2f;
                Gizmos.DrawLine(info.position, progressEnd);
            }
        }
    }
}
