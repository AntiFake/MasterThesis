using UnityEngine;
using System.Collections;
using MasterProject.Core;
using System.Collections.Generic;
using System.Linq;
using MasterProject.VisualDebug;
using MasterProject.NavMesh;

namespace MasterProject.Agent
{
    /// <summary>
    /// Часть класса, в которой реализуется демонстрация "Агента".
    /// </summary>
    public partial class Agent : MonoBehaviour
    {
        private List<RayDebug> raysDebug;
        private NavMeshDebug navMeshDebug;

        /// <summary>
        /// Отображение точек.
        /// </summary>
        private void DrawPoints()
        {
            if (observedPoints != null && observedPoints.Any())
            {
                foreach (KeyValuePair<int, List<Point3D>> ptsSet in observedPoints)
                {
                    foreach (Point3D pt in ptsSet.Value)
                    {
                        Gizmos.color = pt.PointColor;
                        Gizmos.DrawCube((Vector3)pt.position, new Vector3(0.1f, 0.1f, 0.1f));
                    }
                }
            }
        }

        /// <summary>
        /// Отображение лучей сканеров.
        /// </summary>
        public void DrawScannerRays()
        {
            if (raysDebug != null && raysDebug.Any())
            {
                foreach (RayDebug r in raysDebug)
                {
                    Debug.DrawRay(r.origin, r.direction, r.color);
                }
            }
        }

        /// <summary>
        /// Отображение.
        /// </summary>
        public void OnDrawGizmos()
        {
            DrawPoints();

            // Отображение треугольников
            if (passableArea != null)
                navMeshDebug.DrawTriangles(Color.green, new Color(0f, 0f, 150f), passableArea);

            // Отображение контура
            if (contours != null)
                navMeshDebug.DrawContours(Color.magenta, contours);
        }

        /// <summary>
        /// Тестовый интерфейс.
        /// </summary>
        public void OnGUI()
        {
            if (GUI.Button(new Rect(0f, 0f, 80, 30), "Excl"))
                ExcludeExtraPts();
            if (GUI.Button(new Rect(0f, 50f, 80, 30), "Approx."))
                ApproximatePoints();

            if (GUI.Button(new Rect(0f, 150f, 200f, 30f), "Contour"))
            {
                contours.Add(new Contour(observedPoints, new string[] { }));
                //contours.Add(new Contour(observedPoints));
                //c = new Contour(observedPoints, new string[] { });
                //foreach (var contour in contours)
                //{
                //    passableArea.AddRange(triangulator.TriangulateArea(contour, groundScannerInt3Pos));
                //}

                //contours.Add(new Contour(observedPoints));
                //var slopes = Contour.Slopes(observedPoints);
                //foreach (var slope in slopes)
                //{
                //    contours.Add(new Contour(slope, ""));
                //}

                //foreach (var contour in contours)
                //{
                //    passableArea.AddRange(triangulator.TriangulateArea(contour));
                //}
            }

            if (GUI.Button(new Rect(0f, 200f, 200f, 30f), "Triangulate"))
            {
                foreach (var contour in contours)
                {
                    passableArea.AddRange(triangulator.TriangulateArea(contour, groundScannerInt3Pos));
                }
            }

            //if (GUI.Button(new Rect(0f, 200f, 200f, 30f), "StepByStep"))
            //{
            //    triangulator.SBS_TriangulateArea(observedPoints, ref c, ref passableArea);
            //}
        }
    }
}
