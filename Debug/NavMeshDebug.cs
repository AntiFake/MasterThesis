using UnityEngine;
using System.Collections.Generic;
using MasterProject.NavMesh;

namespace MasterProject.VisualDebug
{
    public class NavMeshDebug
    {
        /// <summary>
        /// Отображение контура в Gizmos.
        /// </summary>
        /// <param name="color">Цвет контура</param>
        public void DrawContour(Color color, Contour contour)
        {
            int i = contour.currentPoint.measurementAngle;
            Gizmos.color = color;

            do
            {
                Gizmos.DrawLine((Vector3)contour.currentPoint.point.position, (Vector3)contour.currentPoint.nextPoint.point.position);
                Gizmos.DrawCube((Vector3)contour.currentPoint.point.position, new Vector3(0.1f, 0.1f, 0.1f));
                contour.currentPoint = contour.currentPoint.nextPoint;
            } while (i != contour.currentPoint.measurementAngle);
        }

        /// <summary>
        /// Отображение треугольников, составляющих NavMesh.
        /// </summary>
        /// <param name="color">Цвет треугольников.</param>
        /// <param name="triangles">Треугольники.</param>
        public void DrawTriangles(Color color, List<Triangle> triangles)
        {
            Gizmos.color = color;

            foreach (var triangle in triangles)
            {
                Gizmos.DrawLine((Vector3)triangle.pt1.position, (Vector3)triangle.pt2.position);
                Gizmos.DrawLine((Vector3)triangle.pt2.position, (Vector3)triangle.pt3.position);
                Gizmos.DrawLine((Vector3)triangle.pt3.position, (Vector3)triangle.pt1.position);
            }
        }
    }
}
