using UnityEngine;
using MasterProject.Core;
using MasterProject.Agent;
using System.Collections.Generic;
using System;

namespace MasterProject.NavMesh
{
    // Пища для размышления.
    // https://drpexe.com/a-pathfinding-using-navmesh/
    // http://www.blackpawn.com/texts/pointinpoly/default.html

    public class NavMeshGraphNode : IEquatable<NavMeshGraphNode>
    {
        public Triangle triangle;
        public Guid guid;

        public bool Equals(NavMeshGraphNode node)
        {
            return node != null && node.guid == guid;
        }

        public override int GetHashCode()
        {
            return guid.GetHashCode();
        }
    }

    public class NavMeshGraph
    {
        private Dictionary<NavMeshGraphNode, List<NavMeshGraphNode>> graph;

        public Dictionary<NavMeshGraphNode, List<NavMeshGraphNode>> Graph
        {
            get
            {
                return graph;
            }
        }
        public NavMeshGraph(List<Triangle> triangles, string foo)
        {
            graph = new Dictionary<NavMeshGraphNode, List<NavMeshGraphNode>>();
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

                    edge = AreNeighbours(triangles[i], triangles[j]);

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
            NavMeshGraphNode node = new NavMeshGraphNode()
            {
                triangle = triangle_1,
                guid = triangle_1.guid
            };

            if (!graph.ContainsKey(node))
                graph.Add(node, new List<NavMeshGraphNode>() { });

            graph[node].Add(new NavMeshGraphNode()
            {
                triangle = triangle_2,
                guid = triangle_2.guid
            });
        }

        private List<Point3D> AreNeighbours(Triangle t_1, Triangle t_2)
        {
            List<Point3D> edge = new List<Point3D>();

            if (t_1.pt_1 == t_2.pt_1 || t_1.pt_1 == t_2.pt_2 || t_1.pt_1 == t_2.pt_3)
                edge.Add(t_1.pt_1);

            if (t_1.pt_2 == t_2.pt_1 || t_1.pt_2 == t_2.pt_2 || t_1.pt_2 == t_2.pt_3)
                edge.Add(t_1.pt_2);

            if (t_1.pt_3 == t_2.pt_1 || t_1.pt_3 == t_2.pt_2 || t_1.pt_3 == t_2.pt_3)
                edge.Add(t_1.pt_3);

            return edge;
        }
    }

    // http://www.redblobgames.com/pathfinding/a-star/implementation.html
    public class AStarPathfinding
    {
        private void AStarSearch(Dictionary<NavMeshGraphNode,List<NavMeshGraphNode>> graph, Agent.Agent agent, Point3D start, Point3D goal)
        {
            Queue<Point3D> frontier = new Queue<Point3D>();
            frontier.Enqueue(start);

            List<Point3D> cameFrom = new List<Point3D>();
            List<double> costSoFar = new List<double>();
            

            Point3D current;

            while (frontier.Count > 0)
            {
                current = frontier.Dequeue();

                if (current == goal)
                    break;
                
                
            }
        }

        private double Heuristic(Point3D pt_1, Point3D pt_2)
        {
            return Math.Abs(pt_1.position.x - pt_2.position.x)
                   + Math.Abs(pt_1.position.y - pt_2.position.y)
                   + Math.Abs(pt_1.position.z - pt_2.position.z);
        }
    }
}
