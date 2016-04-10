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
                navMeshDebug.DrawTriangles(Color.blue, new Color(0f, 255f, 0f), passableArea);

            // Отображение контура
            if (contours != null)
                navMeshDebug.DrawContours(Color.red, contours);
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

            if (GUI.Button(new Rect(0f, 150f, 200f, 30f), "ImmediateTest"))
            {
                contours.Add(new Contour(observedPoints));
                var slopes = Contour.Slopes(observedPoints);
                foreach (var slope in slopes)
                {
                    contours.Add(new Contour(slope, ""));
                }

                foreach (var contour in contours)
                {
                    passableArea.AddRange(triangulator.TriangulateArea(contour));
                }
            }

            //if (GUI.Button(new Rect(0f, 200f, 200f, 30f), "StepByStep"))
            //{
            //    triangulator.SBS_TriangulateArea(observedPoints, ref contour, ref passableArea);
            //}

            if (GUI.Button(new Rect(0f, 250f, 200f, 30f), "Slopes"))
            {
                var slopes = Contour.Slopes(observedPoints);
                foreach (var slope in slopes)
                {
                    //contour = new Contour(slope, "");
                    //passableArea = triangulator.TriangulateArea(contour);    
                }
            }
        }
    }
}
