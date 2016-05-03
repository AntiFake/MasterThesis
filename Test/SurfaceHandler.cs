using UnityEngine;
using System.Collections.Generic;

namespace MastersLomasters
{
    public class SurfaceHandler : MonoBehaviour
    {
        #region public variables
        
        // Тип описывающей фигуры.
        public MeshType type;
        // Цвет для OnDrawGizmos.
        public Color gizmoColor = Color.red;
        // Цвет для заливки описывающей фигуры.
        public Color fillGizmoColor = new Color(0f,255f,0f,0.4f);

        #region MeshType = rectangle
        // Размерность сечения описывающей фигуры.
        public Vector2 rectangleSize = new Vector2(1, 1);
        #endregion
        // Высота описывающей фигуры.
        public float height = 1f;
        // Центр описывающей фигуры.
        public Vector3 center;
       
        #endregion

        #region private variables

        // Меш объекта.
        private Mesh mesh;
        // Геометрические границы объекта.
        private Bounds bounds;

        private Dictionary<Int2, int> edges;
        private Dictionary<int, int> pointers;

        #endregion

        public void Awake()
        {
            edges = new Dictionary<Int2, int>();
            pointers = new Dictionary<int, int>();
            mesh = ((MeshFilter)GetComponent("MeshFilter")).sharedMesh;
        }

        /// <summary>
        /// Отображение описывающей фигуры
        /// </summary>
        public void OnDrawGizmos()
        {
            List<List<IntPoint>> buffer = ListPool<List<IntPoint>>.Claim();
            GetContour(buffer);
            Gizmos.color = gizmoColor;
            Bounds bounds = GetBounds();
            float ymin = bounds.min.y;
            Vector3 yoffset = Vector3.up * (bounds.max.y - ymin);

            Gizmos.DrawCube(transform.position, new Vector3(1f, 1f, 1f));

            for (int i = 0; i < buffer.Count; i++)
            {
                List<IntPoint> points = buffer[i];
                for (int j = 0; j < points.Count; j++)
                {
                    Vector3 p1 = IntPointToV3(points[j]);
                    p1.y = ymin;
                    // Когда остаток от деления = 0, p2 означает исходную точку
                    Vector3 p2 = IntPointToV3(points[(j + 1) % points.Count]);
                    p2.y = ymin;
                    Gizmos.DrawLine(p1, p2);
                    Gizmos.DrawLine(p1 + yoffset, p2 + yoffset);
                    Gizmos.DrawLine(p1, p1 + yoffset);
                    Gizmos.DrawLine(p2, p2 + yoffset);
                }
            }

            ListPool<List<IntPoint>>.Release(buffer);
        }

        /// <summary>
        /// Отображение "заполненения" описывающий объект фигуры (при выделении в редакторе Unity)
        /// </summary>
        public void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.Lerp(gizmoColor, fillGizmoColor, 1f);

            Bounds b = GetBounds();
            Gizmos.DrawCube(b.center, b.size);
            Gizmos.DrawWireCube(b.center, b.size);
        }

        #region Функции

        private void CalculateMeshContour()
        {
            if (mesh == null) return;

            edges.Clear();
            pointers.Clear();

            Vector3[] vertices = mesh.vertices;
            int[] triangles = mesh.triangles;

            //Группирует все ребра по принадлежности к треугольникам-полигонам
            for (int i = 0; i < triangles.Length; i += 3)
            {
                edges[new Int2(triangles[i], triangles[i + 1])] = i;
                edges[new Int2(triangles[i + 1], triangles[i + 2])] = i;
                edges[new Int2(triangles[i + 2], triangles[i])] = i;
            }

            //описание всех граней (0-1, 1-2, 2-3, 3-0)
            for (int i = 0; i < triangles.Length; i += 3)
            {
                for (int j = 0; j < 3; j++)
                {
                    if (!edges.ContainsKey(new Int2(triangles[i + ((j + 1) % 3)], triangles[i + j % 3])))
                    {
                        pointers[triangles[i + j % 3]] = triangles[i + (j + 1) % 3];
                    }
                }
            }

            // Список граней.
            List<Vector3[]> contourBuffer = new List<Vector3[]>();
            // Список точек граней.
            List<Vector3> buffer = ListPool<Vector3>.Claim();
            // Группируем вершины по принадлежности к грани.
            for (int i = 0; i < vertices.Length; i++)
            {
                // i - номер вершины,
                // Есть ли вершина с индексом i в словаре вершин.
                if (pointers.ContainsKey(i))
                {
                    buffer.Clear();
                    // Запоминаем текущий индекс начальной вершины грани.
                    int s = i;
                    // Движемся по принципу 0-1, 1-2, 2-3, 3-1, выход из цикла.
                    do
                    {
                        // Получение следующей точки по индексу начальной. Т.е. [0] = 1, Берется 1.
                        int tmp = pointers[s];

                        // Проверка на повторное использование точки при движении по ребрам грани.
                        if (tmp == -1) break;

                        // Обозначение того, что точка уже была пройдена.
                        pointers[s] = -1;

                        // Записываем в буфер вершину (Vector3).
                        buffer.Add(vertices[s]);

                        // ВОЗМОЖНО КУСОК НЕ ИМЕЕТ СМЫСЛА
                        s = tmp;
                        if (s == -1)
                        {
                            Debug.LogError("Invalid Mesh '" + mesh.name + " in " + gameObject.name);
                            break;
                        }
                    } while (s != i);

                    // Если в буфере есть информация по точкам грани, записываем ее в информацию о контуре
                    if (buffer.Count > 0)
                        contourBuffer.Add(buffer.ToArray());
                }
            }
        }

        /// <summary>
        /// Получить границы mesh-а.
        /// </summary>
        /// <returns> Возвращает границы mesh-a. </returns>
        public Bounds GetBounds()
        {
            switch (type)
            {
                case MeshType.Rectangle:
                    bounds = new Bounds(transform.position + center, new Vector3(rectangleSize.x, height, rectangleSize.y));
                    break;
                case MeshType.Circle:
                    // TODO
                    break;
                case MeshType.CustomMesh:
                    // TODO
                    break;
            }
            return bounds;
        }

        /// <summary>
        /// Получить контур проекции объекта на плоскость XZ.
        /// </summary>
        public void GetContour(List<List<IntPoint>> buffer)
        {
            Vector3 offset = transform.position;
            switch (type)
            {
                case MeshType.Rectangle:
                    List<IntPoint> buffer0 = ListPool<IntPoint>.Claim();

                    offset += center;
                    buffer0.Add(V3ToIntPoint(offset + new Vector3(-rectangleSize.x, 0, -rectangleSize.y) * 0.5f));
                    buffer0.Add(V3ToIntPoint(offset + new Vector3(rectangleSize.x, 0, -rectangleSize.y) * 0.5f));
                    buffer0.Add(V3ToIntPoint(offset + new Vector3(rectangleSize.x, 0, rectangleSize.y) * 0.5f));
                    buffer0.Add(V3ToIntPoint(offset + new Vector3(-rectangleSize.x, 0, rectangleSize.y) * 0.5f));

                    buffer.Add(buffer0);
                    break;
                case MeshType.Circle:
                    // TODO
                    break;
                case MeshType.CustomMesh:
                    // TODO
                    break;
            }
        }

        /// <summary>
        /// Приводит тип Vector3 к IntPoint
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        public IntPoint V3ToIntPoint(Vector3 v)
        {
            Int3 ipt = (Int3)v;
            return new IntPoint(ipt.x, ipt.z);
        }

        /// <summary>
        /// Приводит тип IntPoint к Vector3
        /// </summary>
        /// <param name="ipt"></param>
        /// <returns></returns>
        public Vector3 IntPointToV3(IntPoint ipt)
        {
            Int3 ip = new Int3((int)ipt.X, 0, (int)ipt.Y);
            return (Vector3)ip;
        }

        #endregion
    }
}