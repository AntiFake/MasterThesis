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
        private Vector3 mousePos;
               
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
            if (navMesh != null)
                navMeshDebug.DrawNavMesh(Color.green, new Color(0f, 0f, 150f), navMesh);

            // Отображение контура
            if (contours != null)
                navMeshDebug.DrawContours(Color.magenta, contours);

            Gizmos.color = Color.green;
            Gizmos.DrawCube(mousePos, new Vector3(0.5f, 0.5f, 0.5f));
        }

        /// <summary>
        /// Тестовый интерфейс.
        /// </summary>
        public void OnGUI()
        {
            if (GUI.Button(new Rect(0f, 0f, 80, 30), "Excl"))
                measurementApproximation.MinimizeObservedPoints(this);
            if (GUI.Button(new Rect(0f, 50f, 80, 30), "Approx."))
                generalApproximation.ApproximatePoints(this);

            if (GUI.Button(new Rect(0f, 150f, 200f, 30f), "Contour"))
            {
                foreach (var c in outlineBuilder.GetOutlines(this))
                {
                    contours.Add(new Contour(c));
                }
            }

            if (GUI.Button(new Rect(0f, 200f, 200f, 30f), "Triangulate"))
            {
                foreach (var contour in contours)
                {
                    passableArea.AddRange(triangulator.TriangulateArea(contour));
                }

                navMesh = new NavMeshGraph(passableArea);
            }
        }

        public void Update()
        {
            // Позиция мышки.
            if (Input.GetMouseButtonDown(0))
            {
                RaycastHit hit;
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out hit))
                    mousePos = hit.point;

                if (navMesh != null)
                {
                    foreach (var link in navMesh.Graph)
                    {
                        if (GeneralGeometry.IsPointInsideTriangle(link.node_1.triangle, mousePos))
                        {
                            Debug.Log("T_1" + link.node_1.triangle.Center);
                            break;
                        }

                        if (GeneralGeometry.IsPointInsideTriangle(link.node_2.triangle, mousePos))
                        {
                            Debug.Log("T_2" + link.node_2.triangle.Center);
                            break;
                        }
                    }
                }
            }
        }
    }
}
