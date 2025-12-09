using System.Collections.Generic;
using UnityEngine;

namespace _SLIME.Gameplay.Slime.Scripts.new_scripts
{
    public class SlimeConnectionsVisuals
    {
        private readonly SlimeConfiguration _slimeConfig;
        private GameObject _linesFather;
        private NormalLineVisual _lineVisual;

        private readonly Dictionary<(NewConnectingJoint, NewConnectingJoint), (LineVisualizer,LineVisualizer,LineVisualizer)> _lineVisualizers =
            new Dictionary<(NewConnectingJoint, NewConnectingJoint), (LineVisualizer,LineVisualizer,LineVisualizer)>();

        private List<LineVisualizer> _breakingLines;

        public SlimeConnectionsVisuals(SlimeConfiguration slimeConfiguration)
        {
            _slimeConfig = slimeConfiguration;
            _linesFather = new GameObject("LinesFather");
            _lineVisual = new NormalLineVisual();
            _breakingLines = new List<LineVisualizer>();
        }
        
        public void AddVisualLine(NewConnectingJoint connectorOne, NewConnectingJoint connectorTwo)
        {
            LineVisualizer visualizerTop = new LineVisualizer
                (_slimeConfig.LineDefaultSettings, _linesFather.transform, 
                    connectorOne.Top, connectorTwo.Mid, _lineVisual
                    ,connectorOne.name + " Top To " + connectorTwo.name  + " Mid" );
            LineVisualizer visualizerMid = new LineVisualizer
                (_slimeConfig.LineDefaultSettings, _linesFather.transform, 
                    connectorOne.Mid, connectorTwo.Bottom, _lineVisual,
                    connectorOne.name + " Mid To " + connectorTwo.name  + " Bottom" );
            LineVisualizer visualizerBottom = new LineVisualizer
                (_slimeConfig.LineDefaultSettings, _linesFather.transform, 
                    connectorOne.Bottom, connectorTwo.Top, _lineVisual,
                    connectorOne.name + " Bottom To " + connectorTwo.name  + " Top" );
            _lineVisualizers.Add((connectorOne, connectorTwo), 
                (visualizerTop, visualizerMid, visualizerBottom));
        }

        public void FixedUpdate()
        {
            foreach (var e in _lineVisualizers)
            {
                e.Value.Item1.FixedUpdate();  
                e.Value.Item2.FixedUpdate();  
                e.Value.Item3.FixedUpdate();  
            }
        }

        public void LateUpdate()
        {
            foreach (var e in _lineVisualizers)
            {
                e.Value.Item1.LateUpdate();  
                e.Value.Item2.LateUpdate();  
                e.Value.Item3.LateUpdate();   
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

        public void RemoveSegment(NewConnectingJoint connectorOne, NewConnectingJoint connectorTwo)
        {
            AddToBreakingLines(_lineVisualizers[(connectorOne, connectorTwo)].Item1.Remove());
            AddToBreakingLines(_lineVisualizers[(connectorOne, connectorTwo)].Item2.Remove());
            AddToBreakingLines(_lineVisualizers[(connectorOne, connectorTwo)].Item3.Remove());
            _lineVisualizers.Remove((connectorOne, connectorTwo));
        }

        private void AddToBreakingLines(List<LineVisualizer> newParts)
        {
            if (newParts != null) _breakingLines.AddRange(newParts);
        }
    }
}