using UnityEngine;

namespace MasterProject.VisualDebug
{
    public class RayDebug
    {
        public Color color;
        public Vector3 direction;
        public Vector3 origin;

        public RayDebug(Color c, Vector3 o, Vector3 d)
        {
            color = c;
            direction = d;
            origin = o;
        }
    }
}
