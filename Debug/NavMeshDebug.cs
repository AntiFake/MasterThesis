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
                (Vector3)triangle.pt_1.position,
                (Vector3)triangle.pt_2.position,
                (Vector3)triangle.pt_3.position
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
        public void DrawNavMesh(Color outlineColor, Color fillColor, NavMeshGraph navMesh)
        {
            foreach (var link in navMesh.Graph)
            {
                Gizmos.color = outlineColor;
                DrawTriangle(link.node_1.triangle);
                DrawTriangle(link.node_2.triangle);

                Gizmos.color = Color.red;
                Gizmos.DrawLine(link.node_1.triangle.Center, link.node_2.triangle.Center);
            }
        }

        /// <summary>
        /// Отображение треугольника navMesh.
        /// </summary>
        /// <param name="t">Треугольник.</param>
        private void DrawTriangle(Triangle t)
        {
            Gizmos.DrawLine((Vector3) t.pt_1.position, (Vector3) t.pt_2.position);
            Gizmos.DrawLine((Vector3)t.pt_2.position, (Vector3)t.pt_3.position);
            Gizmos.DrawLine((Vector3)t.pt_3.position, (Vector3)t.pt_1.position);
        }
    }
}
