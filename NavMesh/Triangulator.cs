using UnityEngine;
using System.Collections.Generic;
using MasterProject.Core;
using System;

namespace MasterProject.NavMesh
{
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

    public class Triangulator
    {
        public List<Triangle> TriangulateArea(Contour contour)
        {
            List<Triangle> triangles = new List<Triangle>();
            Int3 a, b, c, ab, ba, bc, ac;
            Vector3 b_a, b_b, normal, a_normal, b_normal;
            Plane p;
            float angle1, angle2;

            do
            {
                // Точки треугольника и его плоскость.
                a = contour.currentPoint.point.position;
                b = contour.currentPoint.nextPoint.point.position;
                c = contour.currentPoint.nextPoint.nextPoint.point.position;
                p = new Plane((Vector3)a, (Vector3)b, (Vector3)c);

                // Векторы сторон. ac - проверяемая сторона.
                ab = b - a;
                ba = a - b;
                bc = c - b;
                ac = c - a;

                // Биссектриссы углов A и B треугольника.
                b_a = ab.Normal + ac.Normal;
                b_b = ba.Normal + bc.Normal;

                // Нормаль к плоскости треугольника.
                normal = new Vector3(Math.Abs(p.normal.x), Math.Abs(p.normal.y), Math.Abs(p.normal.z));

                // Вектора, перпендикулярные нормали и сооотв. вектору-стороне треугольника.
                a_normal = Vector3.Cross(normal, (Vector3)ab);
                b_normal = Vector3.Cross(normal, (Vector3)bc);

                angle1 = Vector3.Angle(b_a, a_normal);
                angle2 = Vector3.Angle(b_b, b_normal);

                // Условие триангуляции.
                if (angle1 < 90 && angle2 < 90)
                {
                    triangles.Add(
                        new Triangle(contour.currentPoint.point,
                                    contour.currentPoint.nextPoint.point,
                                    contour.currentPoint.nextPoint.nextPoint.point
                    ));
                    contour.MoveForward(1);
                    contour.DeleteCurrent(true);
                }
                else
                    contour.MoveForward(1);
            }
            while (contour.Count != 3);

            // Добавляем оставшийся треугольник.
            triangles.Add(new Triangle(contour.currentPoint.point,
                contour.currentPoint.nextPoint.point,
                contour.currentPoint.nextPoint.nextPoint.point
            ));

            // Удаление контура
            contour.DeleteCurrent(null);

            return triangles;
        }
    }
}
