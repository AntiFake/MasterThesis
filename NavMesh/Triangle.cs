using MasterProject.Core;
using UnityEngine;
using System;

namespace MasterProject.NavMesh
{
    /// <summary>
    /// Треугольник.
    /// </summary>
    public class Triangle
    {
        public Point3D pt_1;
        public Point3D pt_2;
        public Point3D pt_3;
        public Guid guid;

        /// <summary>
        /// Центр тяжести треугольника.
        /// </summary>
        public Vector3 Center
        {
            get
            {
                float x = pt_1.position.x + pt_2.position.x + pt_3.position.x;
                float y = pt_1.position.y + pt_2.position.y + pt_3.position.y;
                float z = pt_1.position.z + pt_2.position.z + pt_3.position.z;
                return new Vector3(x / 3 / Int3.FloatPrecision, y / 3 / Int3.FloatPrecision, z / 3 / Int3.FloatPrecision);
            }
        }

        public Triangle(Point3D pt1, Point3D pt2, Point3D pt3)
        {
            this.pt_1 = pt1;
            this.pt_2 = pt2;
            this.pt_3 = pt3;
            guid = Guid.NewGuid();
        }
    }
}
