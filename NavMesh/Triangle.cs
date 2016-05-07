using MasterProject.Core;

namespace MasterProject.NavMesh
{
    /// <summary>
    /// Треугольник.
    /// </summary>
    public class Triangle
    {
        public Point3D pt1;
        public Point3D pt2;
        public Point3D pt3;

        public Triangle(Point3D pt1, Point3D pt2, Point3D pt3)
        {
            this.pt1 = pt1;
            this.pt2 = pt2;
            this.pt3 = pt3;
        }
    }
}
