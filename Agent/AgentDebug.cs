using UnityEngine;
using System.Collections;
using MasterProject.Core;
using System.Collections.Generic;
using System.Linq;
using MasterProject.VisualDebug;
using MasterProject.NavMesh;
using System;

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

        private void DrawPath()
        {
            Gizmos.color = Color.cyan;
            for (int i = 0; i < path.Count - 1; i++)
            {
                Gizmos.DrawLine(path[i], path[i + 1]);
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

            // Отображение пути
            if (path != null && path.Any())
            {
                DrawPath();
            }

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

                //AStarPathfinding a = new AStarPathfinding();
                //Guid g1 = navMesh.Graph.Keys.ToList()[0];
                //Guid g2 = navMesh.Graph.Keys.ToList()[navMesh.Graph.Keys.Count - 1];
                //path = a.SearchPath(navMesh.Graph, g1, g2);
            }
        }

        public void Update()
        {
            // Позиция мышки.
            if (Input.GetMouseButtonDown(0) && navMesh != null)
            {
                path.Clear();

                RaycastHit hit;
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out hit))
                    mousePos = hit.point;

                Guid start = GetAgentTriangleGuid();
                Guid end = Guid.NewGuid();

                bool isIn = false;
                if (navMesh != null)
                {
                    foreach (var node in navMesh.Graph)
                    {
                        if (GeneralGeometry.IsPointInsideTriangle(node.Value.triangle, mousePos))
                        {
                            Debug.Log("Node center: " + node.Value.triangle.Center);
                            end = node.Value.triangle.guid;
                            isIn = true;
                            break;
                        }
                    }
                }

                if (isIn)
                {
                    path.Add(groundScannerTransform.position);

                    if (start != end)
                        path.AddRange(aStar.SearchPath(navMesh.Graph, start, end));

                    path.Add(mousePos);
                }
            }
        }

        private void FixedUpdate()
        {
            if (path.Count > 0)
            {
                if (transform.position != path[0])
                    transform.position = Vector3.MoveTowards(transform.position, path[0], speed * Time.deltaTime);
                else
                    path.RemoveAt(0);

                //  + (transform.position - groundScannerTransform.position)
            }
        }

        private Guid GetAgentTriangleGuid()
        {
            return navMesh.Graph.First(i => GeneralGeometry.IsPointInsideTriangle(i.Value.triangle, groundScannerTransform.position)).Key;
        }
    }
}
