using System.Reflection;
using _SLIME.Slime;
using UnityEngine;

namespace _SLIME.Boss
{
    public class BossSpawnAttackDebugger : MonoBehaviour
    {
        [SerializeField] private Animator bossAnimator;
        [SerializeField] private Color targetLineColor = Color.red;
        [SerializeField] private Color slimeAColor = Color.green;
        [SerializeField] private Color slimeBColor = Color.blue;
        [SerializeField] private Color spawnAreaColor = Color.yellow;
        [SerializeField] private float gizmoSphereRadius = 0.5f;
        
        private BossSpawnAttackBehaviour _spawnAttackBehaviour;
        private FieldInfo _spellCounterField;
        private FieldInfo _timerField;
        private FieldInfo _dataField;
        private Vector2 _scrollPosition;
        
        private void Start()
        {
            if (bossAnimator == null)
            {
                bossAnimator = GetComponent<Animator>();
            }
            
            if (bossAnimator != null)
            {
                var behaviours = bossAnimator.GetBehaviours<BossSpawnAttackBehaviour>();
                if (behaviours.Length > 0)
                {
                    _spawnAttackBehaviour = behaviours[0];
                    CacheReflectionFields();
                }
            }
        }
        
        private void CacheReflectionFields()
        {
            var type = typeof(BossSpawnAttackBehaviour);
            var baseType = typeof(BossBaseBehaviour);
            
            _spellCounterField = type.GetField("_spellCounter", BindingFlags.NonPublic | BindingFlags.Instance);
            _timerField = type.GetField("_timer", BindingFlags.NonPublic | BindingFlags.Instance);
            _dataField = baseType.GetField("Data", BindingFlags.NonPublic | BindingFlags.Instance);
        }
        
        private void OnGUI()
        {
            if (_spawnAttackBehaviour == null) return;
            
            GUILayout.BeginArea(new Rect(10, 10, 370, 400));
            _scrollPosition = GUILayout.BeginScrollView(_scrollPosition, "box");
            
            GUILayout.Label("<b>BossSpawnAttackBehaviour Debug</b>", new GUIStyle(GUI.skin.label) { richText = true });
            GUILayout.Space(10);
            
            // Static field
            GUILayout.Label($"TotalAttacksPreformed: {BossBaseBehaviour.TotalAttacksPreformed}");
            
            // Instance fields via reflection
            if (_spellCounterField != null)
            {
                int spellCounter = (int)_spellCounterField.GetValue(_spawnAttackBehaviour);
                GUILayout.Label($"Spell Counter: {spellCounter}");
            }
            
            if (_timerField != null)
            {
                float timer = (float)_timerField.GetValue(_spawnAttackBehaviour);
                GUILayout.Label($"Timer: {timer:F2}");
            }
            
            if (_dataField != null)
            {
                var data = _dataField.GetValue(_spawnAttackBehaviour) as BossBrain;
                if (data != null && data.bossConfigurations != null)
                {
                    GUILayout.Space(5);
                    GUILayout.Label("<b>SpawnAttack Config:</b>", new GUIStyle(GUI.skin.label) { richText = true });
                    GUILayout.Label($"  Spells To Cast: {data.bossConfigurations.SpawnAttack.spellsToCast}");
                    GUILayout.Label($"  Spawn Interval: {data.bossConfigurations.SpawnAttack.spawnInterval}");
                    GUILayout.Label($"  Spell Speed: {data.bossConfigurations.SpawnAttack.spellSpeed}");
                    GUILayout.Label($"  Spell LifeTime: {data.bossConfigurations.SpawnAttack.spellLifeTime}");
                    
                    // Target info
                    GUILayout.Space(5);
                    GUILayout.Label("<b>Target Info:</b>", new GUIStyle(GUI.skin.label) { richText = true });
                    DisplayTargetInfo(data);
                }
                else
                {
                    GUILayout.Label("Data: null (state not active)");
                }
            }
            
            GUILayout.EndScrollView();
            GUILayout.EndArea();
        }
        
        private void DisplayTargetInfo(BossBrain data)
        {
            if (SlimeData.instance == null)
            {
                GUILayout.Label("  SlimeData not available");
                return;
            }
            
            Vector3 slime1Pos = SlimeData.instance.SideATransform.parent.position;
            Vector3 slime2Pos = SlimeData.instance.SideBTransform.parent.position;
            
            bool sideADead = SlimeData.instance.SideADead;
            bool sideBDead = SlimeData.instance.SideBDead;
            
            GUILayout.Label($"  Slime A Dead: {sideADead}");
            GUILayout.Label($"  Slime B Dead: {sideBDead}");
            
            string targetSlime;
            Vector3 targetPos;
            
            if (sideBDead)
            {
                targetSlime = "Slime A (B is dead)";
                targetPos = slime1Pos;
            }
            else if (sideADead)
            {
                targetSlime = "Slime B (A is dead)";
                targetPos = slime2Pos;
            }
            else
            {
                Vector3 spawnPos = data.leftSpawnPoint.position;
                float dist1 = Vector3.Distance(spawnPos, slime1Pos);
                float dist2 = Vector3.Distance(spawnPos, slime2Pos);
                
                if (dist1 < dist2)
                {
                    targetSlime = "Slime A (closer)";
                    targetPos = slime1Pos;
                }
                else
                {
                    targetSlime = "Slime B (closer)";
                    targetPos = slime2Pos;
                }
                
                GUILayout.Label($"  Dist to A: {dist1:F2}");
                GUILayout.Label($"  Dist to B: {dist2:F2}");
            }
            
            GUILayout.Label($"  <color=yellow>Target: {targetSlime}</color>", new GUIStyle(GUI.skin.label) { richText = true });
            GUILayout.Label($"  Target Pos: {targetPos}");
        }
        
        private void OnDrawGizmos()
        {
            if (!Application.isPlaying) return;
            if (_spawnAttackBehaviour == null || _dataField == null) return;
            
            var data = _dataField.GetValue(_spawnAttackBehaviour) as BossBrain;
            if (data == null || SlimeData.instance == null) return;
            
            Vector3 slimeAPos = SlimeData.instance.SideATransform.parent.position;
            Vector3 slimeBPos = SlimeData.instance.SideBTransform.parent.position;
            Vector3 spawnPos = data.leftSpawnPoint.position;
            
            // Draw slime positions
            bool sideADead = SlimeData.instance.SideADead;
            bool sideBDead = SlimeData.instance.SideBDead;
            
            Gizmos.color = sideADead ? Color.gray : slimeAColor;
            Gizmos.DrawWireSphere(slimeAPos, gizmoSphereRadius);
            
            Gizmos.color = sideBDead ? Color.gray : slimeBColor;
            Gizmos.DrawWireSphere(slimeBPos, gizmoSphereRadius);
            
            // Calculate target
            Vector3 targetPos;
            if (sideBDead)
            {
                targetPos = slimeAPos;
            }
            else if (sideADead)
            {
                targetPos = slimeBPos;
            }
            else
            {
                float dist1 = Vector3.Distance(spawnPos, slimeAPos);
                float dist2 = Vector3.Distance(spawnPos, slimeBPos);
                targetPos = dist1 < dist2 ? slimeAPos : slimeBPos;
            }
            
            // Draw line from spawn to target
            Gizmos.color = targetLineColor;
            Gizmos.DrawLine(spawnPos, targetPos);
            Gizmos.DrawSphere(targetPos, gizmoSphereRadius * 0.3f);
            
            // Draw spawn area
            if (data.leftSpawnPoint != null && data.rightSpawnPoint != null)
            {
                Gizmos.color = spawnAreaColor;
                Vector3 leftPos = data.leftSpawnPoint.position;
                Vector3 rightPos = data.rightSpawnPoint.position;
                Gizmos.DrawLine(leftPos, rightPos);
                Gizmos.DrawWireSphere(leftPos, gizmoSphereRadius * 0.5f);
                Gizmos.DrawWireSphere(rightPos, gizmoSphereRadius * 0.5f);
            }
        }
    }
}
