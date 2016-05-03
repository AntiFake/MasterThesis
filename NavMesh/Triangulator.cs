using UnityEngine;
using System.Collections.Generic;
using MasterProject.Core;
using System;

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

    /// <summary>
    /// Класс, обеспечивающий разбитие области на треуголники.
    /// </summary>
    public class Triangulator
    {
        /// <summary>
        /// Разбитие заданной области на треугольники.
        /// </summary>
        /// <param name="contour">Область</param>
        /// <returns></returns>
        public List<Triangle> TriangulateArea(Contour contour, Int3 groundScannerPos)
        {
            List<Triangle> triangles = new List<Triangle>();
            Int3 a, b, c, ab, ba, bc, ac;
            Vector3 b_a, b_b, normal, a_normal, b_normal;
            Plane p;
            float angle1, angle2;

            int it = 0, limit = 100000;

            do
            {
                if ((contour.currentPoint.point.obstacleName == contour.currentPoint.nextPoint.point.obstacleName
                    && contour.currentPoint.nextPoint.point.obstacleName == contour.currentPoint.nextPoint.nextPoint.point.obstacleName)
                    ||
                    (contour.currentPoint.point.position.y == contour.currentPoint.nextPoint.point.position.y && contour.currentPoint.nextPoint.point.position.y == contour.currentPoint.nextPoint.nextPoint.point.position.y)
                    )
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
                else
                    contour.MoveForward(1);

                if (it == limit)
                {
                    Debug.Log("Точек без контура! " + contour.Count);
                    break;
                }
                it++;
            }
            while (contour.Count != 3);

            // Если возможно, то добавляем оставшийся треугольник.
            if ((contour.currentPoint.point.obstacleName == contour.currentPoint.nextPoint.point.obstacleName
                   && contour.currentPoint.nextPoint.point.obstacleName == contour.currentPoint.nextPoint.nextPoint.point.obstacleName)
                   ||
                   (contour.currentPoint.point.position.y == contour.currentPoint.nextPoint.point.position.y && contour.currentPoint.nextPoint.point.position.y == contour.currentPoint.nextPoint.nextPoint.point.position.y)
                   )
            {
                triangles.Add(
                    new Triangle(contour.currentPoint.point,
                        contour.currentPoint.nextPoint.point,
                        contour.currentPoint.nextPoint.nextPoint.point
                ));
            }

            // Удаление контура
            contour.DeleteCurrent(null);

            return triangles;
        }

        /// <summary>
        /// Демонстрация алгоритма триангуляции.
        /// </summary>
        /// <param name="list"></param>
        /// <param name="contour"></param>
        /// <param name="triangles"></param>
        /// <returns></returns>
        public List<Triangle> SBS_TriangulateArea(Dictionary<int, List<Point3D>> list, ref Contour contour, ref List<Triangle> triangles)
        {
            if (contour == null)
                contour = new Contour(list);

            if (triangles == null)
                triangles = new List<Triangle>();

            // Выход из алгоритма.
            if (contour.Count < 3)
                return triangles;

            // Добавление оставшегося треугольника и удаление контура.
            if (contour.Count == 3)
            {
                triangles.Add(new Triangle(contour.currentPoint.point,
                    contour.currentPoint.nextPoint.point,
                    contour.currentPoint.nextPoint.nextPoint.point
                ));
            
                contour.DeleteCurrent(null);

                return triangles;
            }

            // ====== Шаг алгоритма триангуляции. =======

            if ((contour.currentPoint.point.obstacleName == contour.currentPoint.nextPoint.point.obstacleName
                   && contour.currentPoint.nextPoint.point.obstacleName == contour.currentPoint.nextPoint.nextPoint.point.obstacleName)

                   ||

                   (contour.currentPoint.point.position.y == contour.currentPoint.nextPoint.point.position.y && contour.currentPoint.nextPoint.point.position.y == contour.currentPoint.nextPoint.nextPoint.point.position.y)
                   )
            {


                Int3 a, b, c, ab, ba, bc, ac;
                Vector3 b_a, b_b, normal, a_normal, b_normal;
                Plane p;
                float angle1, angle2;

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

                return triangles;
            }
            else
            {
                Debug.Log("ВЫшел!");
                contour.MoveForward(1);
                return triangles;
            }
        }
    }
}
