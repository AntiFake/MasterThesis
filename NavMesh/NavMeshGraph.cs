using UnityEngine;
using MasterProject.Core;
using MasterProject.Agent;
using System.Linq;
using System.Collections.Generic;
using System;

namespace MasterProject.NavMesh
{
    public class NavMeshGraphNode
    {
        public Triangle triangle;
        public List<Triangle> neighbours;
    }

    public class NavMeshGraph
    {
        private Dictionary<Guid, NavMeshGraphNode> graph;

        public Dictionary<Guid, NavMeshGraphNode> Graph
        {
            get
            {
                return graph;
            }
        }
        public NavMeshGraph(List<Triangle> triangles)
        {
            graph = new Dictionary<Guid, NavMeshGraphNode>();
            BuildGraph(triangles);
        }

        private void BuildGraph(List<Triangle> triangles)
        {
            List<Point3D> edge = new List<Point3D>();
            int neighboursCount;

            for (int i = 0; i < triangles.Count; i++)
            {
                neighboursCount = 0;
                for (int j = 0; j < triangles.Count; j++)
                {
                    if (triangles[i] == triangles[j])
                        continue;

                    edge = GeneralGeometry.AreNeighbours(triangles[i], triangles[j]);

                    // Треугольники имеют одну смежную сторону.
                    if (edge.Count == 2)
                    {
                        AddGraphNode(triangles[i], triangles[j]);
                        neighboursCount++;
                    }

                    if (neighboursCount == 3)
                        break;
                }
            }
        }

        private void AddGraphNode(Triangle triangle_1, Triangle triangle_2)
        {

            if (!graph.ContainsKey(triangle_1.guid))
                graph.Add(triangle_1.guid, new NavMeshGraphNode()
                {
                    triangle = triangle_1,
                    neighbours = new List<Triangle>()
                });

            graph[triangle_1.guid].neighbours.Add(triangle_2);
        }
    }

    public class AStarPathfinding
    {
        public class PathNode
        {
            // ID.
            public Guid guid;

            // Треугольник NavMesh.
            public Triangle triangle;

            // Координаты точки.
            public Vector3 position;

            // Длина пути от старта (G - оценка).
            public double pathLengthFromStart;

            // Точка, из которой пришли в эту точку.
            public PathNode cameFrom;

            // Примерное расстояние до цели (H - оценка).
            public double heuristicEstimatePathLength;
            
            // Ожидаемое полное расстояние до конечной точки (F).
            public double EstimateFullPathLength
            {
                get
                {
                    return pathLengthFromStart + heuristicEstimatePathLength;
                }
            }

        }

        public List<Vector3> SearchPath(Dictionary<Guid, NavMeshGraphNode> graph, Guid start, Guid goal)
        {
            // 1. Открытый и закрытый списки.
            List<PathNode> closedList = new List<PathNode>();
            List<PathNode> openedList = new List<PathNode>();

            NavMeshGraphNode goalNode = graph[goal];
            NavMeshGraphNode startNode = graph[start];

            // 2. Создание первой точки пути.
            PathNode startPathNode = new PathNode()
            {
                guid = start,
                triangle = startNode.triangle,
                position = startNode.triangle.Center,
                cameFrom = null,
                pathLengthFromStart = 0,
                heuristicEstimatePathLength = GetHeuristicPathLength(startNode.triangle.Center, goalNode.triangle.Center),
            };
            openedList.Add(startPathNode);

            PathNode currentPathNode = null;
            PathNode openPathNode = null;

            while (openedList.Count > 0)
            {
                // 3. Выбор точки с наименьшим F.
                currentPathNode = openedList.OrderBy(i => i.EstimateFullPathLength).First();

                // 4. Если текущая точка и есть цель поиска.
                if (currentPathNode.guid == goal)
                    return GetPath(currentPathNode);

                // 5. Перемещение текущей точки из списка ожидающих рассмотрение в уже рассмотренные.
                openedList.Remove(currentPathNode);
                closedList.Add(currentPathNode);

                // 6. Для каждой из соседних для текущей точек:
                foreach (var neighbourPathNode in GetNeighbours(graph, currentPathNode, goalNode))
                {
                    currentPathNode = neighbourPathNode;

                    // Шаг 7. Если Y уже находится в рассмотренных – пропускаем ее.
                    if (closedList.Count(i => i.guid == neighbourPathNode.guid) > 0)
                        continue;

                    openPathNode = openedList.FirstOrDefault(i => i.guid == neighbourPathNode.guid);
                    // Шаг 8. Если Y еще нет в списке на ожидание – добавляем ее туда, 
                    // запомнив ссылку на X и рассчитав Y.G (это X.G + расстояние от X до Y) и Y.H.
                    if (openPathNode == null)
                        openedList.Add(neighbourPathNode);
                    else
                    {
                        if (openPathNode.pathLengthFromStart > neighbourPathNode.pathLengthFromStart)
                        {
                            // Шаг 9. Если же Y в списке на рассмотрение – проверяем, если X.G + расстояние от X до Y < Y.G, 
                            // значит мы пришли в точку Y более коротким путем, заменяем Y.G на X.G + расстояние от X до Y, 
                            // а точку, из которой пришли в Y на X.
                            openPathNode.cameFrom = currentPathNode;
                            openPathNode.triangle = currentPathNode.triangle;
                            openPathNode.pathLengthFromStart = neighbourPathNode.pathLengthFromStart;
                        }
                    }
                }
            }

            return null;
        }

        private List<PathNode> GetNeighbours(Dictionary<Guid, NavMeshGraphNode> graph, PathNode currentPathNode, NavMeshGraphNode goalNode)
        {
            List<PathNode> pathNodes = new List<PathNode>();

            foreach (var neighbour in graph[currentPathNode.guid].neighbours)
            {
                pathNodes.Add(new PathNode()
                {
                    guid = neighbour.guid,
                    triangle = neighbour,
                    position = neighbour.Center,
                    cameFrom = currentPathNode,
                    pathLengthFromStart = currentPathNode.pathLengthFromStart + GetDistanceBetweenNeighbours(currentPathNode.position, neighbour.Center),
                    heuristicEstimatePathLength = GetHeuristicPathLength(neighbour.Center, goalNode.triangle.Center)
                });    
            }

            return pathNodes;
        }

        private double GetHeuristicPathLength(Vector3 center_1, Vector3 center_2)
        {
            return Math.Abs(center_1.x - center_2.x)
                    + Math.Abs(center_1.y - center_2.y)
                    + Math.Abs(center_1.z - center_2.z);
        }


        private float GetDistanceBetweenNeighbours(Vector3 center_1, Vector3 center_2)
        {
            return (center_1 - center_2).magnitude;
        }

        private static List<Vector3> GetPath(PathNode pathNode)
        {
            List<Vector3> resultPath = new List<Vector3>();
            List<Point3D> edge = new List<Point3D>();
            PathNode currentPathNode = pathNode;

            while (currentPathNode != null)
            {
                resultPath.Add(currentPathNode.position);

                // Поиск сопряженного ребра.
                if (currentPathNode.cameFrom != null)
                {
                    edge = GeneralGeometry.AreNeighbours(currentPathNode.triangle, currentPathNode.cameFrom.triangle);

                    if (edge.Count != 2)
                        throw new Exception("Два смежных треугольника не имеют общей стороны");

                    resultPath.Add(GeneralGeometry.GetEdgeCenter(edge[0], edge[1]));
                }

                currentPathNode = currentPathNode.cameFrom;
            }

            resultPath.Reverse();
            return resultPath;
        }
    }
}
