using System.Collections.Generic;
using UnityEngine;

namespace _SLIME.Gameplay.Slime.Scripts.new_scripts
{
    public interface IVisualStrategy
    {
        public bool UpdateLineVisuals(ref LineVisualizerComponents components, LineSettings lineSettings);
        
        public List<LineVisualizer> AnimateRemovalOfLine(ref LineVisualizerComponents components,
            LineSettings lineSettings);
    }
}