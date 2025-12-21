using System.Collections.Generic;
using UnityEngine;

namespace _SLIME.Slime
{
    public interface IVisualStrategy
    {
        public bool UpdateLineVisuals(ref LineVisualizerComponents components, LineSettings lineSettings);
        
        public List<LineVisualizer> AnimateRemovalOfLine(ref LineVisualizerComponents components,
            LineSettings lineSettings);
    }
}