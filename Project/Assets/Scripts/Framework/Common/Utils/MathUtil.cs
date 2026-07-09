using UnityEngine;

namespace Framework.Utils
{
    public class MathUtil
    {
        public static bool IsSphereOverlap(float x, float y, float z, float radius1, float x2, float y2, float z2,
            float radius2)
        {
            return (x - x2) * (x - x2) + (y - y2) * (y - y2) + (z - z2) * (z - z2) < (radius1 + radius2) * (radius1 + radius2);
        }

        public static bool IsSphereOverlap(Vector3 pos, float radius1, Vector3 pos2,
            float radius2)
        {
            return IsSphereOverlap(pos.x, pos.y, pos.z, radius1, pos2.x, pos2.y, pos2.z, radius2);
        }
    }
}