using MasterProject.Core;
using System;

namespace MasterProject.NavMesh
{
    /// <summary>
    /// Класс, описывающий точку контура.
    /// </summary>
    public class ContourPoint
    {
        public Guid uniqueIndex;
        public Point3D point;
        public ContourPoint nextPoint;
        public ContourPoint prevPoint;

        public ContourPoint(Guid ma, Point3D pt)
        {
            uniqueIndex = ma;
            point = pt;
        }
    }
}
