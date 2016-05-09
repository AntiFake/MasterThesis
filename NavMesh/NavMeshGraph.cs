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

    public class NavMeshGraphEdge
    {
        public Point3D point_1;
        public Point3D point_2;
    }

    public class NavMeshGraphNode
    {
        public Triangle triangle;
        public Guid guid;
    }

    public class NavMeshGraphLink
    {
        public NavMeshGraphNode node_1;
        public NavMeshGraphNode node_2;
        public NavMeshGraphEdge edge;
    }

    public class NavMeshGraph
    {
        private List<NavMeshGraphLink> graph;
        public List<NavMeshGraphLink> Graph
        {
            get
            {
                return graph;
            }
        }

        public NavMeshGraph(List<Triangle> triangles)
        {
            graph = new List<NavMeshGraphLink>();
            while (triangles.Count != 0)
            {
                BuildGraph(triangles, triangles[0]);
            }
        }

        private void BuildGraph(List<Triangle> triangles, Triangle triangle)
        {
            triangles.Remove(triangle);
            for (int i = 0; i < triangles.Count; i++)
            {
                var edge = AreNeighbours(triangle, triangles[i]);
                if (edge.Count == 2)
                    AddGraphLink(triangle, triangles[i], edge[0], edge[1]);
            }
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

        private void AddGraphLink(Triangle triangle_1, Triangle triangle_2, Point3D pt_1, Point3D pt_2)
        {
            graph.Add(
                new NavMeshGraphLink()
                {
                    node_1 = new NavMeshGraphNode()
                    {
                        triangle = triangle_1,
                        guid = Guid.NewGuid()
                    },
                    node_2 = new NavMeshGraphNode()
                    {
                        triangle = triangle_2,
                        guid = Guid.NewGuid()
                    },
                    edge = new NavMeshGraphEdge()
                    {
                        point_1 = pt_1,
                        point_2 = pt_2
                    }
                }
            );
        }
    }

    public class AStarPathfinding
    {
        private Vector3 GetAgentPosition(Agent.Agent agent)
        {
            return new Vector3();
        }
    }
}
