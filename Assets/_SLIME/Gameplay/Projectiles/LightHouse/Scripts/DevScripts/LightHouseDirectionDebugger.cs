using System;
using System.Reflection;
using UnityEngine;

namespace _SLIME.LightHouse
{
    public class LightHouseDirectionDebugger : MonoBehaviour
    {
        private LightHouseAttackLogic _logic;
        private Type _logicType;
        private FieldInfo _mainDirectionEffectiveField;
        private FieldInfo _mainBeamIndexField;
        private FieldInfo _transitionInProgressField;
        private FieldInfo _lightHouseSetsField;
        private FieldInfo _lightHouseDepsField;
        private MethodInfo _getDesiredMainDirectionMethod;
        private MethodInfo _getFurthestSlimeMethod;
        private MethodInfo _getMainBeamRotationPointMethod;
        private MethodInfo _directionToLaserRotationZMethod;
        private MethodInfo _normalizeAngleMethod;

        private float _slimeAngle;
        private float _beamAngle;
        private float _delta;
        private float _deadZone;
        private int _desiredDirection;
        private bool _inDeadZone;
        private string _furthestSlimeName;
        private Vector3 _centerPos;
        private Vector3 _slimePos;

        private void Awake()
        {
            _logicType = typeof(LightHouseAttackLogic);
            _mainDirectionEffectiveField = _logicType.GetField("_mainDirectionEffective", BindingFlags.NonPublic | BindingFlags.Instance);
            _mainBeamIndexField = _logicType.GetField("_mainBeamIndex", BindingFlags.NonPublic | BindingFlags.Instance);
            _transitionInProgressField = _logicType.GetField("_transitionInProgress", BindingFlags.NonPublic | BindingFlags.Instance);
            _lightHouseSetsField = _logicType.GetField("_lightHouseSets", BindingFlags.NonPublic | BindingFlags.Instance);
            _lightHouseDepsField = _logicType.GetField("lightHouseDeps", BindingFlags.NonPublic | BindingFlags.Instance);
            _getDesiredMainDirectionMethod = _logicType.GetMethod("GetDesiredMainDirection", BindingFlags.NonPublic | BindingFlags.Instance);
            _getFurthestSlimeMethod = _logicType.GetMethod("GetFurthestSlime", BindingFlags.NonPublic | BindingFlags.Instance);
            _getMainBeamRotationPointMethod = _logicType.GetMethod("GetMainBeamRotationPoint", BindingFlags.NonPublic | BindingFlags.Instance);
            _directionToLaserRotationZMethod = _logicType.GetMethod("DirectionToLaserRotationZ", BindingFlags.NonPublic | BindingFlags.Static);
            _normalizeAngleMethod = _logicType.GetMethod("NormalizeAngle", BindingFlags.NonPublic | BindingFlags.Static);
        }

        private void Update()
        {
            _logic = FindObjectOfType<LightHouseAttackLogic>(true);
            if (_logic == null || !_logic.gameObject.activeInHierarchy) return;

            UpdateDirectionDebugValues();
        }

        private void UpdateDirectionDebugValues()
        {
            try
            {
                var depsBoxed = _lightHouseDepsField?.GetValue(_logic);
                if (depsBoxed == null) return;
                LightHouseDeps d = (LightHouseDeps)depsBoxed;

                var furthest = _getFurthestSlimeMethod?.Invoke(_logic, null) as Transform;
                if (furthest == null)
                {
                    _furthestSlimeName = "null";
                    _desiredDirection = 0;
                    return;
                }

                _furthestSlimeName = furthest.name;
                _centerPos = d.lighthouseCenter.position;
                _slimePos = furthest.position;

                Vector2 toSlime = (Vector2)(_slimePos - _centerPos);
                float atan2Deg = Mathf.Atan2(toSlime.y, toSlime.x) * Mathf.Rad2Deg;
                if (_directionToLaserRotationZMethod == null) return;
                _slimeAngle = (float)_directionToLaserRotationZMethod.Invoke(null, new object[] { atan2Deg });

                var mainRotation = _getMainBeamRotationPointMethod?.Invoke(_logic, null) as Transform;
                if (mainRotation == null) return;

                float rawBeamZ = mainRotation.eulerAngles.z;
                if (_normalizeAngleMethod == null) return;
                _beamAngle = (float)_normalizeAngleMethod.Invoke(null, new object[] { rawBeamZ });

                _delta = Mathf.DeltaAngle(_beamAngle, _slimeAngle);

                var sets = _lightHouseSetsField?.GetValue(_logic);
                float directionFlipDeadZone = 8f;
                if (sets != null)
                {
                    var setsType = sets.GetType();
                    var dzField = setsType.GetField("directionFlipDeadZone");
                    if (dzField != null) directionFlipDeadZone = (float)dzField.GetValue(sets);
                }
                _deadZone = Mathf.Max(1f, directionFlipDeadZone);
                _inDeadZone = Mathf.Abs(_delta) < _deadZone;

                if (_getDesiredMainDirectionMethod != null)
                    _desiredDirection = (int)_getDesiredMainDirectionMethod.Invoke(_logic, null);
            }
            catch (Exception ex)
            {
                _furthestSlimeName = $"Error: {ex.Message}";
            }
        }

        private void OnGUI()
        {
            if (_logic == null || !_logic.gameObject.activeInHierarchy)
            {
                GUILayout.BeginArea(new Rect(10, Screen.height - 120, 400, 110));
                GUI.backgroundColor = Color.gray;
                GUILayout.Box("LightHouse Direction Debugger - LightHouse not active");
                GUILayout.EndArea();
                return;
            }

            float mainDirEff = _mainDirectionEffectiveField != null ? (float)_mainDirectionEffectiveField.GetValue(_logic) : 0;
            int mainBeamIdx = _mainBeamIndexField != null ? (int)_mainBeamIndexField.GetValue(_logic) : 0;
            bool transition = _transitionInProgressField != null && (bool)_transitionInProgressField.GetValue(_logic);

            float h = 260;
            GUILayout.BeginArea(new Rect(10, Screen.height - h - 10, 420, h));
            GUI.backgroundColor = new Color(0.2f, 0.2f, 0.3f, 0.95f);
            GUILayout.BeginVertical(GUI.skin.box);

            GUILayout.Label("<b>LightHouse GetDesiredMainDirection Debug</b>", RichLabel());
            GUILayout.Space(4);

            GUILayout.Label($"slimeAngle (laser Z): {_slimeAngle:F2}°");
            GUILayout.Label($"beamAngle (normalized): {_beamAngle:F2}°");
            GUILayout.Label($"delta (DeltaAngle): {_delta:F2}°  [path to slime]");
            GUILayout.Label($"deadZone: {_deadZone:F1}°  |  inDeadZone: {_inDeadZone}");
            GUILayout.Label($"desiredDirection (1/-1): {_desiredDirection}");
            GUILayout.Space(4);

            GUILayout.Label($"mainDirectionEffective: {mainDirEff:F3}");
            GUILayout.Label($"mainBeamIndex: {mainBeamIdx}");
            GUILayout.Label($"transitionInProgress: {transition}");
            GUILayout.Space(4);

            GUILayout.Label($"furthestSlime: {_furthestSlimeName}");
            GUILayout.Label($"center: {_centerPos}  slime: {_slimePos}");

            GUILayout.EndVertical();
            GUILayout.EndArea();
        }

        private static GUIStyle RichLabel()
        {
            var s = new GUIStyle(GUI.skin.label) { richText = true, fontStyle = FontStyle.Bold };
            return s;
        }
    }
}
