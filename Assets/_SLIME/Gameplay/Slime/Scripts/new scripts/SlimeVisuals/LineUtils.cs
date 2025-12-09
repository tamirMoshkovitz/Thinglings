

using UnityEngine;

namespace _SLIME.Gameplay.Slime.Scripts.new_scripts
{
    public static class LineUtils
    {
        public static void ApplyParabolaInterpolation(LineVisualizerComponents components, LineSettings lineSettings,
            Vector3 startPos, Vector3 endPos, float currentSag)
        {
            for (int i = 0; i < lineSettings.visualResolution; i++)
            {
                float k = (float)i / (lineSettings.visualResolution - 1);
                
                Vector3 point = Vector3.Lerp(startPos, endPos, k);

                point.y -= 4 * k * (1 - k) * currentSag;

                components.linePositions[i] = point;
            }
        }
    }
}