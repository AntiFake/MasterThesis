using System.Collections.Generic;
using UnityEngine;

namespace MasterProject.Core
{
    /// <summary>
    /// keyPt - ключевая точка, 
    /// nonkeyPt - не ключевая точка, 
    /// sealedPt - "бесплодная" точка, точки которые могут создавать только 2 связи с точками соседних словарей (если они также бесплодны). Все остальные связи строятся с участием данных точек не в качестве источника.
    /// extraPt - лишняя (избыточная) точка
    /// </summary>
    public enum Point3DType { keyPt, nonkeyPt, sealedPt, extraPt }

    public class Point3D
    {
        // Для сортировки по вертикали.
        public int index_y;
        public Int3 position;
        public string obstacleName;
        public Point3DType type;

        public Color PointColor
        {
            get
            {
                switch (type)
                {
                    case (Point3DType.keyPt): return Color.green;
                    case (Point3DType.nonkeyPt): return Color.yellow;
                    case (Point3DType.sealedPt): return Color.red;
                    case (Point3DType.extraPt): return Color.blue;
                    default: return Color.gray;
                }
            }
        }

        public Point3D(Int3 pos, string obstacleName, Point3DType t = Point3DType.nonkeyPt)
        {
            position = pos;
            this.obstacleName = obstacleName;
            type = t;
        }

        public static explicit operator Point3D(Vector3 pt)
        {
            return new Point3D((Int3)pt, null);
        }

        public static Point3D operator -(Point3D pt_1, Point3D pt_2)
        {
            return new Point3D(pt_1.position - pt_2.position, pt_1.obstacleName, pt_1.type);
        }

        public static Point3D operator /(Point3D pt, int divider)
        {
            return new Point3D(pt.position / divider, pt.obstacleName, pt.type);
        }
    }

    internal class Point3DEqualityComparer : IEqualityComparer<Point3D>
    {
        public bool Equals(Point3D pt1, Point3D pt2)
        {
            return pt1.position.x == pt2.position.x &&
                    pt1.position.y == pt2.position.y &&
                    pt1.position.z == pt2.position.z;
        }

        public int GetHashCode(Point3D pt)
        {
            return (int)pt.position.x;
        }
    }
}
