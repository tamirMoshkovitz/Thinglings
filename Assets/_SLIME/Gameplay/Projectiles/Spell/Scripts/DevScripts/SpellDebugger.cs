using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace _SLIME.Projectiles
{
    public class SpellDebugger : MonoBehaviour
    {
        [Header("Gizmo Settings")]
        [SerializeField] private Color velocityColor = Color.green;
        [SerializeField] private Color directionColor = Color.cyan;
        [SerializeField] private Color spawningColor = Color.yellow;
        [SerializeField] private Color flyingColor = Color.red;
        [SerializeField] private Color hitColor = Color.gray;
        [SerializeField] private float velocityScale = 0.5f;
        [SerializeField] private float sphereRadius = 0.3f;
        
        // Cached reflection info
        private FieldInfo _currentStateField;
        private FieldInfo _bossAttributesField;
        private FieldInfo _compField;
        private FieldInfo _moveSpeedField;
        private FieldInfo _directionField;
        private FieldInfo _rbField;
        private FieldInfo _animatorField;
        private FieldInfo _colliderField;
        
        private Vector2 _scrollPosition;
        private List<SpellDebugInfo> _spellInfos = new List<SpellDebugInfo>();
        private List<SpellDebugInfo> _destroyedSpellsHistory = new List<SpellDebugInfo>();
        
        [Header("History Settings")]
        [SerializeField] private int maxHistoryCount = 10;
        
        // Track last velocity before hit for each spell
        private Dictionary<int, Vector2> _lastVelocityBeforeHit = new Dictionary<int, Vector2>();
        private Dictionary<int, SpellState> _previousStates = new Dictionary<int, SpellState>();
        
        private struct SpellDebugInfo
        {
            public Spell spell;
            public int instanceId;
            public string spellName;
            public Vector3 position;
            public Vector2 velocity;
            public float speed;
            public SpellState state;
            public Vector3 bossDirection;
            public float bossMoveSpeed;
            public string layerName;
            public int layer;
            public Vector2 lastVelocityBeforeHit;
            public bool hasLastVelocity;
            public string animatorStateName;
            public float animatorNormalizedTime;
            public bool isValid;
            public bool isDestroyed;
        }
        
        private void Start()
        {
            CacheReflectionFields();
        }
        
        private void CacheReflectionFields()
        {
            var spellType = typeof(Spell);
            var bossAttrType = typeof(SpellBossAttributes);
            var compType = typeof(SpellComp);
            
            _currentStateField = spellType.GetField("_currentState", BindingFlags.NonPublic | BindingFlags.Instance);
            _bossAttributesField = spellType.GetField("_bossAttributes", BindingFlags.NonPublic | BindingFlags.Instance);
            _compField = spellType.GetField("comp", BindingFlags.NonPublic | BindingFlags.Instance);
            
            _moveSpeedField = bossAttrType.GetField("moveSpeed", BindingFlags.Public | BindingFlags.Instance);
            _directionField = bossAttrType.GetField("direction", BindingFlags.Public | BindingFlags.Instance);
            
            _rbField = compType.GetField("rb", BindingFlags.Public | BindingFlags.Instance);
            _animatorField = compType.GetField("animator", BindingFlags.Public | BindingFlags.Instance);
            _colliderField = compType.GetField("collider", BindingFlags.Public | BindingFlags.Instance);
        }
        
        private void Update()
        {
            UpdateSpellInfos();
        }
        
        private void UpdateSpellInfos()
        {
            _spellInfos.Clear();
            
            var spells = FindObjectsByType<Spell>(FindObjectsSortMode.None);
            
            foreach (var spell in spells)
            {
                if (spell == null) continue;
                
                int instanceId = spell.GetInstanceID();
                
                var info = new SpellDebugInfo
                {
                    spell = spell,
                    instanceId = instanceId,
                    spellName = spell.name,
                    position = spell.transform.position,
                    isValid = true,
                    isDestroyed = false
                };
                
                // Get state via reflection
                if (_currentStateField != null)
                {
                    info.state = (SpellState)_currentStateField.GetValue(spell);
                }
                
                // Get comp, rigidbody, and animator via reflection
                if (_compField != null)
                {
                    var comp = _compField.GetValue(spell);
                    if (comp != null)
                    {
                        if (_rbField != null)
                        {
                            var rb = (Rigidbody2D)_rbField.GetValue(comp);
                            if (rb != null)
                            {
                                info.velocity = rb.linearVelocity;
                                info.speed = rb.linearVelocity.magnitude;
                            }
                        }
                        
                        if (_animatorField != null)
                        {
                            var animator = (Animator)_animatorField.GetValue(comp);
                            if (animator != null)
                            {
                                var stateInfo = animator.GetCurrentAnimatorStateInfo(0);
                                info.animatorNormalizedTime = stateInfo.normalizedTime;
                                info.animatorStateName = GetAnimatorStateName(animator, stateInfo);
                            }
                        }
                        
                        if (_colliderField != null)
                        {
                            var col = (Collider2D)_colliderField.GetValue(comp);
                            if (col != null)
                            {
                                info.layer = col.gameObject.layer;
                                info.layerName = LayerMask.LayerToName(col.gameObject.layer);
                            }
                        }
                    }
                }
                
                // Track velocity before hit
                SpellState previousState = SpellState.Spawning;
                _previousStates.TryGetValue(instanceId, out previousState);
                
                // If state just changed to Hit, save the last velocity
                if (info.state == SpellState.Hit && previousState != SpellState.Hit)
                {
                    // The velocity we captured might already be zero, so we use the one from previous frame
                    if (_lastVelocityBeforeHit.ContainsKey(instanceId))
                    {
                        // Keep the existing one
                    }
                    else if (info.velocity.sqrMagnitude > 0.01f)
                    {
                        _lastVelocityBeforeHit[instanceId] = info.velocity;
                    }
                }
                // If flying, always update the "potential" last velocity
                else if (info.state == SpellState.Flying && info.velocity.sqrMagnitude > 0.01f)
                {
                    _lastVelocityBeforeHit[instanceId] = info.velocity;
                }
                
                _previousStates[instanceId] = info.state;
                
                // Set last velocity info
                if (_lastVelocityBeforeHit.TryGetValue(instanceId, out Vector2 lastVel))
                {
                    info.lastVelocityBeforeHit = lastVel;
                    info.hasLastVelocity = true;
                }
                
                // Get boss attributes via reflection
                if (_bossAttributesField != null)
                {
                    var bossAttr = _bossAttributesField.GetValue(spell);
                    if (bossAttr != null)
                    {
                        if (_moveSpeedField != null)
                            info.bossMoveSpeed = (float)_moveSpeedField.GetValue(bossAttr);
                        if (_directionField != null)
                            info.bossDirection = (Vector3)_directionField.GetValue(bossAttr);
                    }
                }
                
                _spellInfos.Add(info);
            }
            
            // Cleanup destroyed spells from tracking
            CleanupDestroyedSpells(spells);
        }
        
        // Store the last known info for each spell so we can save it when destroyed
        private Dictionary<int, SpellDebugInfo> _lastKnownInfos = new Dictionary<int, SpellDebugInfo>();
        
        private void CleanupDestroyedSpells(Spell[] currentSpells)
        {
            var currentIds = new HashSet<int>();
            foreach (var spell in currentSpells)
            {
                if (spell != null)
                    currentIds.Add(spell.GetInstanceID());
            }
            
            // Update last known infos
            foreach (var info in _spellInfos)
            {
                _lastKnownInfos[info.instanceId] = info;
            }
            
            var idsToRemove = new List<int>();
            foreach (var id in _previousStates.Keys)
            {
                if (!currentIds.Contains(id))
                    idsToRemove.Add(id);
            }
            
            foreach (var id in idsToRemove)
            {
                // Save to history before removing
                if (_lastKnownInfos.TryGetValue(id, out SpellDebugInfo lastInfo))
                {
                    lastInfo.isDestroyed = true;
                    lastInfo.spell = null;
                    _destroyedSpellsHistory.Insert(0, lastInfo);
                    
                    // Limit history size
                    while (_destroyedSpellsHistory.Count > maxHistoryCount)
                    {
                        _destroyedSpellsHistory.RemoveAt(_destroyedSpellsHistory.Count - 1);
                    }
                    
                    _lastKnownInfos.Remove(id);
                }
                
                _previousStates.Remove(id);
                _lastVelocityBeforeHit.Remove(id);
            }
        }
        
        private void OnGUI()
        {
            float width = 420;
            float height = Screen.height - 20f;
            
            GUILayout.BeginArea(new Rect(10, 10, width, height));
            _scrollPosition = GUILayout.BeginScrollView(_scrollPosition, "box");
            
            var titleStyle = new GUIStyle(GUI.skin.label) { richText = true, fontSize = 14 };
            GUILayout.Label($"<b>Spell Debugger</b>", titleStyle);
            GUILayout.Label($"Active: {_spellInfos.Count} | History: {_destroyedSpellsHistory.Count}");
            GUILayout.Space(10);
            
            // Active spells
            if (_spellInfos.Count > 0)
            {
                GUILayout.Label("<b><color=green>--- Active Spells ---</color></b>", titleStyle);
                for (int i = 0; i < _spellInfos.Count; i++)
                {
                    var info = _spellInfos[i];
                    DrawSpellInfo(info, i, false);
                    GUILayout.Space(5);
                }
            }
            else
            {
                GUILayout.Label("<i>No active spells...</i>", titleStyle);
            }
            
            // History (destroyed spells)
            if (_destroyedSpellsHistory.Count > 0)
            {
                GUILayout.Space(10);
                GUILayout.Label("<b><color=orange>--- Destroyed Spells (History) ---</color></b>", titleStyle);
                for (int i = 0; i < _destroyedSpellsHistory.Count; i++)
                {
                    var info = _destroyedSpellsHistory[i];
                    DrawSpellInfo(info, i, true);
                    GUILayout.Space(5);
                }
            }
            
            GUILayout.EndScrollView();
            GUILayout.EndArea();
        }
        
        private void DrawSpellInfo(SpellDebugInfo info, int index, bool isHistory)
        {
            var headerStyle = new GUIStyle(GUI.skin.label) { richText = true };
            
            string stateColor = info.state switch
            {
                SpellState.Spawning => "yellow",
                SpellState.Flying => "red",
                SpellState.Hit => "gray",
                _ => "white"
            };
            
            GUILayout.BeginVertical("box");
            
            string spellName = info.spellName ?? (info.spell != null ? info.spell.name : "null");
            string prefix = isHistory ? "<color=orange>[DESTROYED]</color> " : "";
            GUILayout.Label($"{prefix}<b><color={stateColor}>Spell {index}: {spellName}</color></b>", headerStyle);
            
            GUILayout.Label($"  State: {info.state}", headerStyle);
            GUILayout.Label($"  <color=cyan>Animator: <b>{info.animatorStateName}</b></color> ({info.animatorNormalizedTime:F2})", headerStyle);
            GUILayout.Label($"  Layer: <b>{info.layerName}</b> ({info.layer})", headerStyle);
            GUILayout.Label($"  Position: {info.position:F2}");
            GUILayout.Label($"  Velocity: {info.velocity:F2}");
            GUILayout.Label($"  Speed: {info.speed:F2}");
            
            // Show last velocity before hit
            if (info.hasLastVelocity)
            {
                float lastSpeed = info.lastVelocityBeforeHit.magnitude;
                string velColor = info.state == SpellState.Hit ? "orange" : "white";
                GUILayout.Label($"  <color={velColor}>Last Velocity Before Hit: {info.lastVelocityBeforeHit:F2}</color>", headerStyle);
                GUILayout.Label($"  <color={velColor}>Last Speed Before Hit: {lastSpeed:F2}</color>", headerStyle);
            }
            
            GUILayout.Label($"  Boss Direction: {info.bossDirection:F2}");
            GUILayout.Label($"  Boss Move Speed: {info.bossMoveSpeed:F2}");
            
            GUILayout.EndVertical();
        }
        
        private void OnDrawGizmos()
        {
            if (!Application.isPlaying) return;
            
            foreach (var info in _spellInfos)
            {
                if (!info.isValid || info.spell == null) continue;
                
                DrawSpellGizmos(info);
            }
        }
        
        private void DrawSpellGizmos(SpellDebugInfo info)
        {
            Vector3 pos = info.position;
            
            // Draw sphere at spell position with state color
            Color stateCol = info.state switch
            {
                SpellState.Spawning => spawningColor,
                SpellState.Flying => flyingColor,
                SpellState.Hit => hitColor,
                _ => Color.white
            };
            
            Gizmos.color = stateCol;
            Gizmos.DrawWireSphere(pos, sphereRadius);
            
            // Draw velocity vector
            if (info.velocity.sqrMagnitude > 0.01f)
            {
                Gizmos.color = velocityColor;
                Vector3 velocityEnd = pos + (Vector3)info.velocity * velocityScale;
                Gizmos.DrawLine(pos, velocityEnd);
                DrawArrowHead(pos, velocityEnd, 0.2f);
            }
            
            // Draw boss direction (original intended direction)
            if (info.bossDirection.sqrMagnitude > 0.01f)
            {
                Gizmos.color = directionColor;
                Vector3 dirEnd = pos + info.bossDirection.normalized * 1.5f;
                Gizmos.DrawLine(pos, dirEnd);
                DrawArrowHead(pos, dirEnd, 0.15f);
            }
        }
        
        private void DrawArrowHead(Vector3 start, Vector3 end, float size)
        {
            Vector3 direction = (end - start).normalized;
            Vector3 right = Quaternion.Euler(0, 0, 30) * -direction * size;
            Vector3 left = Quaternion.Euler(0, 0, -30) * -direction * size;
            
            Gizmos.DrawLine(end, end + right);
            Gizmos.DrawLine(end, end + left);
        }
        
        private string GetAnimatorStateName(Animator animator, AnimatorStateInfo stateInfo)
        {
            // Try to get clip info for the actual animation name
            var clipInfo = animator.GetCurrentAnimatorClipInfo(0);
            if (clipInfo.Length > 0 && clipInfo[0].clip != null)
            {
                return clipInfo[0].clip.name;
            }
            
            // Fallback: Check common spell animation states by name
            string[] stateNames = { "Spawn", "BossMove", "PlayerMove", "Hit", "Idle", "Flying", 
                                    "spawn", "bossMove", "playerMove", "hit", "idle", "flying" };
            
            foreach (var stateName in stateNames)
            {
                if (stateInfo.IsName(stateName))
                    return stateName;
            }
            
            // Return hash if nothing else works
            return $"Hash: {stateInfo.fullPathHash}";
        }
    }
}
