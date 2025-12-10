using System.Collections.Generic;
using UnityEngine;

namespace _SLIME.Gameplay.Slime.Scripts.new_scripts
{
    public class NormalLineVisual: IVisualStrategy
    {
        public bool UpdateLineVisuals(ref LineVisualizerComponents components, LineSettings lineSettings)
        {
            Vector3 startPos = components.lineStart.TransformPoint(components.lineStartLocalPos);
            Vector3 endPos = components.lineEnd.TransformPoint(components.lineEndLocalPos);
            float distance = Vector3.Distance(startPos, endPos);
            
            float tension = Mathf.InverseLerp(lineSettings.closeDistance, lineSettings.farDistance, distance);
            float baseSag = Mathf.Lerp(lineSettings.maxSag, 0f, tension);
            
            float finalSag = baseSag + components.currentVibrationOffset;
            components.lr.widthMultiplier = Mathf.Lerp(lineSettings.maxThickness, lineSettings.minThickness, tension);
            
            LineUtils.ApplyParabolaInterpolation(ref components.linePositions, lineSettings.visualResolution, startPos, endPos, finalSag);
            
            components.lr.SetPositions(components.linePositions);
            return false;
        }
        
        
        

        public List<LineVisualizer> AnimateRemovalOfLine(ref LineVisualizerComponents components,
            LineSettings lineSettings)
        {
            
            Vector3 startPosWorld = components.lineStart.TransformPoint(components.lineStartLocalPos);
            Vector3 endPosWorld = components.lineEnd.TransformPoint(components.lineEndLocalPos);
            Vector3 midPointWorld = (startPosWorld + endPosWorld) * 0.5f;
            Vector3 lineDirection = (endPosWorld - startPosWorld).normalized;
            Vector3 perpendicular = new Vector3(lineDirection.y, -lineDirection.x, 0);
            if (perpendicular.y > 0) perpendicular *= -1;
            midPointWorld += perpendicular * lineSettings.tearDropOfLines;
            LineSettings brokenSettings = lineSettings;
            brokenSettings.maxSag = brokenSettings.maxSag/2; 
            brokenSettings.maxThickness = components.lr.widthMultiplier; 
            Transform father = components.lineObject.gameObject.transform.parent;
            components.lr.enabled = false;
            

            List<LineVisualizer> newParts = new List<LineVisualizer>();
            
            
            newParts.Add(new LineVisualizer(
                brokenSettings, father, 
                components.lineStart,   
                midPointWorld,      
                new BreakingLineVisual(), 
                components.lineObject.name + " Breaking Line 1"
            ));

            newParts.Add(new LineVisualizer(
                brokenSettings, father, 
                components.lineEnd,    
                midPointWorld,        
                new BreakingLineVisual(), 
                components.lineObject.name + " Breaking Line 2"
            ));
            
            Object.Destroy(components.lineObject);

            return newParts;
        }
    }
}