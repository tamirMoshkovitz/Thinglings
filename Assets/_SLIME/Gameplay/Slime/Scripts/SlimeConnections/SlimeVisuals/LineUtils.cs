

using UnityEngine;

namespace _SLIME.Slime
{
    public static class LineUtils
    {
        public static void ApplyParabolaInterpolation(ref Vector3[] linePositions, int resolution,
            Vector3 startPos, Vector3 endPos, float currentSag)
        {
            Vector3 lineDir = (endPos - startPos).normalized;
            Vector3 perpendicular = new Vector3(-lineDir.y, lineDir.x, 0); 
            if (perpendicular.y > 0)
            {
                perpendicular *= -1;
            }
            for (int i = 0; i < resolution; i++)
            {
                float k = (float)i / (resolution - 1);
                
                Vector3 point = Vector3.Lerp(startPos, endPos, k);

                float sagOffset = 4 * k * (1 - k) * currentSag;

                linePositions[i] = point + (perpendicular * sagOffset);
            }
        }
    }
}