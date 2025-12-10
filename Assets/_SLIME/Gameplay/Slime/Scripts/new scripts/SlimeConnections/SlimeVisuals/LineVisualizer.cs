using System.Collections.Generic;
using Player;
using UnityEngine;

namespace _SLIME.Gameplay.Slime.Scripts.new_scripts
{
    
    [System.Serializable]
    public struct LineSettings
    {
        public GameObject linePrefab;
        public int visualResolution;   
        public float maxSag;        
        public float maxThickness;  
        public float minThickness;  
        public float closeDistance;  
        public float farDistance;
        public float tearDuration;
        public float tearDropOfLines;
        
        public float vibrationSpeed; 
        public float vibrationDecay; 
        public float vibrationStrength;
    }
    
    public struct LineVisualizerComponents
    {
        public Transform lineStart;
        public Vector3 lineStartLocalPos;
        public Vector3 lineEndLocalPos;
        public Transform lineEnd;
        public LineRenderer lr;
        public GameObject lineObject;
        public Vector3[] linePositions;
        public float currentVibrationOffset;
    }
    
    public class LineVisualizer
    {
        private LineVisualizerComponents _components;
        private LineSettings _lineSettings;
        private IVisualStrategy _visualStrategy;

        public LineVisualizer(LineSettings lineSettings, Transform linesFather,
            Transform start, Transform end, IVisualStrategy visualStrategy, string name = "")
        {
            InitCommon(lineSettings, linesFather, name, visualStrategy);

            _components.lineStart = start;
            _components.lineEnd = end;
            
            _components.lineStartLocalPos = start.InverseTransformPoint(start.position); 
            _components.lineEndLocalPos = end.InverseTransformPoint(end.position);
            SlimeEvents.TrampolineActivated += TriggerBounce;
        }

        public LineVisualizer(LineSettings lineSettings, Transform linesFather,
            Transform anchor, Vector3 localEndPos, IVisualStrategy visualStrategy, string name = "")
        {
            InitCommon(lineSettings, linesFather, name, visualStrategy);

            _components.lineStart = anchor;
            _components.lineEnd = anchor; 
            
            _components.lineStartLocalPos = Vector3.zero; 
            _components.lineEndLocalPos = localEndPos;    
        }
        
        private void InitCommon(LineSettings settings, Transform father, string name, IVisualStrategy strategy)
        {
            _lineSettings = settings;
            _visualStrategy = strategy;
            
            _components.lineObject = Object.Instantiate(_lineSettings.linePrefab, father);
            _components.lineObject.name = name;
            _components.lineObject.transform.localPosition = Vector3.zero;
            _components.lr = _components.lineObject.GetComponent<LineRenderer>();
            _components.linePositions = new Vector3[_lineSettings.visualResolution];
            _components.lr.positionCount = _lineSettings.visualResolution;
            _components.lr.useWorldSpace = true;
        }
        
        public void TriggerBounce()
        {
            Debug.Log("TriggerBounce");
            _isVibrating = true;
            _vibrationTimer = 0f;
            
            _currentVibrationAmplitude = _lineSettings.vibrationStrength;
        }
        public void FixedUpdate()
        {
            
        }
        public bool LateUpdate()
        {
            if (_isVibrating)
            {
                UpdateVibrationPhysics();
            }
            else
            {
                _components.currentVibrationOffset = 0;
            }
            
            return _visualStrategy.UpdateLineVisuals(ref _components, _lineSettings);
        }

        #region  Testing

        private float _vibrationTimer = 0f;
        private float _currentVibrationAmplitude = 0f;
        private bool _isVibrating = false;
        private void UpdateVibrationPhysics()
        {
            _vibrationTimer += Time.deltaTime;

            float oscillation = Mathf.Sin(_vibrationTimer * _lineSettings.vibrationSpeed);

            
            _components.currentVibrationOffset = oscillation * _currentVibrationAmplitude;

            
            _currentVibrationAmplitude -= _lineSettings.vibrationDecay * Time.deltaTime;

           
            if (_currentVibrationAmplitude <= 0f)
            {
                _currentVibrationAmplitude = 0f;
                _isVibrating = false;
            }
        }

        #endregion
        
        
        public List<LineVisualizer> Remove()
        {
            return _visualStrategy.AnimateRemovalOfLine(ref _components, _lineSettings);
        }
    }
}