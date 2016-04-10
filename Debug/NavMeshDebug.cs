using UnityEngine;
using System.Collections.Generic;
using MasterProject.NavMesh;
using System;

namespace MasterProject.VisualDebug
{
    public class NavMeshDebug
    {
        private long i = 0;

        /// <summary>
        /// Отображение заполненного цветом треугольника треугольника.
        /// </summary>
        /// <param name="triangle"></param>
        /// <returns></returns>
        private Mesh GetTriangleMesh(Triangle triangle)
        {
            Mesh mesh = new Mesh();
            mesh.vertices = new Vector3[3]
            {
                (Vector3)triangle.pt1.position,
                (Vector3)triangle.pt2.position,
                (Vector3)triangle.pt3.position
            };

            mesh.triangles = new int[]
            {
                0,1,2
            };

            mesh.normals = new Vector3[]
            {
                Vector3.forward,
                Vector3.forward,
                Vector3.forward
            };

            return mesh;
        }

        /// <summary>
        /// Отображение всех контуров.
        /// </summary>
        /// <param name="color">Цвет ребер</param>
        /// <param name="contours">Контуры</param>
        public void DrawContours(Color color, List<Contour> contours)
        {
            foreach (var contour in contours)
            {
                DrawContour(color, contour);
            }
        }

        /// <summary>
        /// Отображение контура в Gizmos.
        /// </summary>
        /// <param name="color">Цвет контура</param>
        private void DrawContour(Color color, Contour contour)
        {
            if (contour.currentPoint == null)
                return;

            Guid i = contour.currentPoint.uniqueIndex;
            Gizmos.color = color;

            do
            {
                Gizmos.DrawLine((Vector3)contour.currentPoint.point.position, (Vector3)contour.currentPoint.nextPoint.point.position);
                Gizmos.DrawCube((Vector3)contour.currentPoint.point.position, new Vector3(0.1f, 0.1f, 0.1f));
                contour.currentPoint = contour.currentPoint.nextPoint;
            } while (i != contour.currentPoint.uniqueIndex);
        }

        /// <summary>
        /// Отображение треугольников, составляющих NavMesh.
        /// </summary>
        /// <param name="color">Цвет треугольников.</param>
        /// <param name="triangles">Треугольники.</param>
        public void DrawTriangles(Color outlineColor, Color fillColor, List<Triangle> triangles)
        {
            foreach (var triangle in triangles)
            {
                Gizmos.color = outlineColor;
                Gizmos.DrawLine((Vector3)triangle.pt1.position, (Vector3)triangle.pt2.position);
                Gizmos.DrawLine((Vector3)triangle.pt2.position, (Vector3)triangle.pt3.position);
                Gizmos.DrawLine((Vector3)triangle.pt3.position, (Vector3)triangle.pt1.position);

                Gizmos.color = fillColor;
                //Gizmos.DrawMesh(GetTriangleMesh(triangle));
            }
        }
    }
}
