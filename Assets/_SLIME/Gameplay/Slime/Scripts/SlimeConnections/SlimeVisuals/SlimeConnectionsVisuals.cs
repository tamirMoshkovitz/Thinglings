using System.Collections.Generic;
using UnityEngine;

namespace _SLIME.Slime
{
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

        public SlimeConnectionsVisuals(SlimeConfiguration slimeConfiguration, SlimeData slimeData,
            ConnectionsComponents connectionsComponents)
        {
            _slimeConfig = slimeConfiguration;
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
        }
        
        
        public void LateUpdate()
        {
            if(_visualizeAboveLine != null) _visualizeAboveLine.LateUpdate();
            if(_visualizeBellowLine != null) _visualizeBellowLine.LateUpdate();
            
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
        }

        private void AddToBreakingLines(List<LineVisualizer> newParts)
        {
            if (newParts != null) _breakingLines.AddRange(newParts);
        }
    }
}