using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace _SLIME.Gameplay.Slime.Scripts.new_scripts
{
    public class BreakingLineVisual: IVisualStrategy
    {
        private float _timer = 0;
        public bool UpdateLineVisuals(ref LineVisualizerComponents components, LineSettings lineSettings)
        {
            _timer += Time.deltaTime / lineSettings.tearDuration;
            float t = Mathf.Clamp01(_timer);
            
            Vector3 anchorPos = components.lineStart.TransformPoint(components.lineStartLocalPos);
            
            Vector3 breakOriginPos = components.lineEndLocalPos;
            float t2 = Mathf.Pow(t, 2);
            Vector3 currentTipPos = Vector3.Lerp(breakOriginPos, anchorPos, t2);
            float currentSag = Mathf.Lerp(lineSettings.maxSag, 0f, t);
            components.lr.widthMultiplier = Mathf.Lerp(lineSettings.maxThickness, lineSettings.minThickness, t);
            LineUtils.ApplyParabolaInterpolation(
                ref components.linePositions,
                lineSettings.visualResolution, 
                anchorPos,      
                currentTipPos,  
                currentSag      
            );
            
            components.lr.SetPositions(components.linePositions);
            return _timer >= 1f;
        }

        public List<LineVisualizer> AnimateRemovalOfLine(ref LineVisualizerComponents components,
            LineSettings lineSettings)
        {
            Object.Destroy(components.lineObject);
            return null;
        }
    }
}