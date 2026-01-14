using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace _SLIME.Slime
{
    [Serializable]
    public struct SparkSets
    {
       public AnimationCurve SparkAnimationCurve;
       public float maxScale;
       public float minScale;
    }
    public class SlimeConnectionsVisuals
    {
        private readonly SlimeConfiguration _slimeConfig;
        private GameObject _linesFather;
        private NormalLineVisual _lineVisual;

        
        private List<LineVisualizer> _breakingLines;
        private readonly ConnectionsComponents _connectionsComponents;
        private SlimeData _slimeData;
        private LineVisualizer _visualizeAboveLine;
        private LineVisualizer _visualizeBellowLine;
        private Transform _sparkTransform;
        private readonly SparkSets _sparkSet;
        private List<Transform> _sparkScaleTransforms;

        public SlimeConnectionsVisuals(SlimeConfiguration slimeConfiguration, SlimeData slimeData,
            ConnectionsComponents connectionsComponents)
        {
            _slimeConfig = slimeConfiguration;
            _sparkSet = _slimeConfig.SparkSet;
            _connectionsComponents = connectionsComponents;
            _linesFather = new GameObject("LinesFather");
            _lineVisual = new NormalLineVisual();
            _breakingLines = new List<LineVisualizer>();
            _slimeData = slimeData;
        }
        
        public void AddVisualLine(Transform first, Transform second)
        {
            _visualizeAboveLine = new LineVisualizer
                (_slimeConfig.LineAboveSettings, _linesFather.transform, 
                    first,second, _lineVisual, _slimeData
                    ,"ConnectionAboveLine" );
            _visualizeBellowLine = new LineVisualizer
            (_slimeConfig.LineBellowSettings, _linesFather.transform, 
                first,second, _lineVisual, _slimeData
                ,"ConnectionBellowLine" );
            _sparkTransform = Object.Instantiate(_connectionsComponents.Spark,
                _visualizeAboveLine.GetLineCenter(), Quaternion.identity).transform;
            
            _sparkScaleTransforms = new List<Transform>();
            foreach (Transform child in _sparkTransform.GetComponentsInChildren<Transform>(true))
            {
                if (child.CompareTag("SparkScale"))
                {
                    _sparkScaleTransforms.Add(child);
                }
            }
        }
        
        
        public void LateUpdate()
        {
            if(_visualizeAboveLine != null) _visualizeAboveLine.LateUpdate();
            if(_visualizeBellowLine != null) _visualizeBellowLine.LateUpdate();
            if (_sparkTransform != null)
            {
                _sparkTransform.position = _visualizeAboveLine.GetLineCenter();
                
                float curveValue = _sparkSet.SparkAnimationCurve.Evaluate(_slimeData.StretchRatio);
                float scale = Mathf.Lerp(_sparkSet.minScale, _sparkSet.maxScale, curveValue);
                Vector3 scaleVector = Vector3.one * scale;
                
                foreach (Transform t in _sparkScaleTransforms)
                {
                    t.localScale = scaleVector;
                }
            }
            for (int i = _breakingLines.Count - 1; i >= 0; i--)
            {
                var breaker = _breakingLines[i];
                if (breaker.LateUpdate())
                {
                    breaker.Remove();
                    _breakingLines.RemoveAt(i);
                }
            }
            
            
        }
        
        public void RemoveSegment()
        {
            AddToBreakingLines(_visualizeAboveLine.Remove(_slimeConfig.LineAboveBreakSettings));
            AddToBreakingLines(_visualizeBellowLine.Remove(_slimeConfig.LineBellowBreakSettings));
            _visualizeAboveLine = null;
            _visualizeBellowLine = null;
            Object.Destroy(_sparkTransform.gameObject);
            _sparkTransform = null;
        }

        private void AddToBreakingLines(List<LineVisualizer> newParts)
        {
            if (newParts != null) _breakingLines.AddRange(newParts);
        }
    }
}