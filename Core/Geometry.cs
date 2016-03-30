using UnityEngine;
using System.Collections;

namespace MasterProject.Core
{
    public static class Geometry
    {
        public static bool CheckByVectorsIfPointBelongsTo3DLine(Int3 pt1, Int3 pt2, Int3 pt3, float error)
        {
            Int3 v1, v2;
            v1 = pt2 - pt1;
            v2 = pt3 - pt1;

            double angle = v1.GetAngleBetweenVectors(v2);

            if (angle >= (-1) * error && angle <= error)
                return true;

            return false;
        }

    }
}
