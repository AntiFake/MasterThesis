// Translate, rotate and scale a mesh. Try varying
// the parameters in the inspector while running
// to see the effect they have.
/*
using UnityEngine;
using System.Collections;

public class TestScript : MonoBehaviour
{
    public Transform tr;
    */

    ///// <summary>
    ///// Обозначение графа возможных путей движения 
    ///// </summary>
    //public abstract class NavGraph
    //{
    //    public List<GraphNode> nodes;

    //}

    //#region GraphNode

    //public delegate void GraphNodeDelegate(GraphNode node);
    //public delegate void GraphNodeDelegateCancelable(GraphNode node);

    //public abstract class GraphNode
    //{
    //    private int nodeIndex;
    //    protected uint nodeFlags;

    //    // Флаги, изображающие ращлиные виды местности. Некоторые затрудняют перемещение 
    //    //private uint penalty; 

    //    // Позиция на карте
    //    public abstract Int3 Position { get; }
    //    // Получение индекса ноды
    //    public int NodeIndex { get { return nodeIndex; } }

    //    // Удаление ноды графа
    //    public void Destroy()
    //    {
    //        if (nodeIndex == -1)
    //            return;
    //        ClearConnections(true);
    //        nodeIndex = -1;
    //    }

    //    // Удаление всех связей ноды с другими нодами графа. 
    //    // Если alsoReverse = true, то удаление производится и в обратном порядке.
    //    public abstract void ClearConnections(bool alsoReverse);
    //    // Функция вызова делегата для получения всех соседних нод графа.
    //    public abstract void GetConnections(GraphNodeDelegate del);
    //    // Функция удаления связи с определенной нодой.
    //    public abstract void RemoveConnection(GraphNode node);
    //    // Функция проверки существования соединения текущей ноды с указанной
    //    public virtual bool ContainsConnection(GraphNode node)
    //    {
    //        bool contains = false;
    //        GetConnections(delegate (GraphNode n)
    //        {
    //            if (n == node) contains = true;
    //        });
    //        return contains;
    //    }

    //    // Функция построения портала между двумя нодами графа.
    //    // public virtual bool GetPortal(GraphNode otherNode, List<Vector3> left, List<Vector3> right, bool backwards)
    //    // {
    //    //     return false;
    //    // }

    //    // Сериализация ноды
    //    //public virtual void SerializeNode(ctx) { }
    //    // Десериализация ноды
    //    //public virtual void DeserializeNode(ctx) { }
    //    // Сериализация соединений с другими нодами
    //    //public virtual void SerializeReferences(ctx) { }
    //    // Десериализция соединений с другими нодами
    //    //public virtual void DeserializeReferences(ctx) { }
    //}
    //#endregion
    /*
    void Start()
    {
        Matrix4x4 m = Matrix4x4.TRS(tr.position, tr.rotation, Vector3.one);
        Matrix4x4 m1 = new Matrix4x4();
        m1.SetColumn(0, tr.right);
        m1.SetColumn(1, tr.up);
        m1.SetColumn(2, tr.forward);
        m1.SetColumn(3, new Vector4(tr.position.x, tr.position.y, tr.position.z, 1));
    }

    void Update()
    {

    }
}



public class Triangle
{
    public Point3D pt1;
    public Point3D pt2;
    public Point3D pt3;

    public Int3 center;
    public double radius;

    public Triangle(Point3D pt1, Point3D pt2, Point3D pt3)
    {
        this.pt1 = pt1;
        this.pt2 = pt2;
        this.pt3 = pt3;
    }
}

public class Edge
{
    // Описание типов ребер.
    // slept - спящее ребро (еще не примавшее участия в алгоритме триангуляции);
    // alive - свободное с одной стороны ребро;
    // dead - замкнутое с двух сторон ребро.
    public enum EdgeType { slept = 1, alive = 2, dead = 3 }

    // Точка 1.
    public Point3D pt1;
    // Точка 2.
    public Point3D pt2;
    // Тип ребра.
    public EdgeType type;

    public Edge(Point3D pt1, Point3D pt2, EdgeType type)
    {
        this.pt1 = pt1;
        this.pt2 = pt2;
        this.type = type;
    }
}

private List<Edge> contour = new List<Edge>();
private List<Point3D> points = new List<Point3D>();

/// <summary>
/// Получаем замкнутый контур. Берем пока только первые точки в словаре.
/// </summary>
/// <param name="ptDict">Словарь точек</param>
private void CreateContour(Dictionary<int, List<Point3D>> ptDict)
{
    points = ptDict.Select(i => i.Value[0]).ToList();

    int[] keys = ptDict.Keys.ToArray();
    Edge edge;

    for (int i = 0; i < keys.Length; i++)
    {
        if (i == keys.Length - 1)
            edge = new Edge(ptDict[keys[i]][0], ptDict[keys[0]][0], Edge.EdgeType.slept);
        else
            edge = new Edge(ptDict[keys[i]][0], ptDict[keys[i + 1]][0], Edge.EdgeType.slept);

        contour.Add(edge);
    }
}

private void DrawContour(Color lineColor)
{
    Gizmos.color = lineColor;
    for (int i = 0; i < contour.Count; i++)
    {
        Gizmos.DrawLine((Vector3)contour[i].pt1.position, (Vector3)contour[i].pt2.position);
    }
}*/

// 1. Выбираем спящее контурное ребро.
// 2. Заносим это ребро в список frontier
// 3. Получаем список всех точек, принадлежащих спящим ребрам.
// 4. Проводим окружности через 2 точки рассматриваемого ребра и доп. точки.
// 5. Сравниваем радиусы окружностей. Выбираем с минимальным.
// 6. Строим два новых ребра из каждой точки рассматриваемого в найденную точку. Ограничение: одно из ребер уже может существовать (его в мертвое), новое построенное в живое.
// 7. Найденные ребра (о) сохраняем в frontier.
// 8. Возвращаемся к шагу 1, и выполняем алгоритм дотех пор, пока frontiers не станет пустым. 

// 1. После построения нового ребра делаем его спящим.
// 2. Проверяем не принадлежит новое ребро другому треугольнику, все ребра которого спящие.
// 3.   Если да, то все три ребра треугольника делаем мертвыми



/*
private List<Triangle> TriangulateSpace()
{
    List<Triangle> triangles = new List<Triangle>();
    List<KeyValuePair<Point3D, double>> circleRadiuses = new List<KeyValuePair<Point3D, double>>();
    KeyValuePair<Point3D, double> point;
    int currentIndex, checkIndex1, checkIndex2;

    if (!contour.Any())
        return null;

    while (contour.Any(i => i.type == Edge.EdgeType.slept))
    {
        // Поиск первого спящего ребра.
        currentIndex = contour.FindIndex(i => i.type == Edge.EdgeType.slept);
        contour[currentIndex].type = Edge.EdgeType.alive;

        // Поиск наиболее выгодной точки для построения треугольника.
        circleRadiuses = points
            .Where(i => !(i.position == contour[currentIndex].pt1.position || i.position == contour[currentIndex].pt2.position))
            .Select(i => new KeyValuePair<Point3D, double>(i, GetCircleRadius(contour[currentIndex], i))).ToList();

        point = circleRadiuses.OrderBy(i => i.Value).FirstOrDefault();

        // Обработка текущего ребра.
        contour[currentIndex].type = Edge.EdgeType.dead;

        // Добавление новых ребер.
        checkIndex1 = contour.FindIndex(i =>
            (i.pt1.position == point.Key.position && (i.pt2.position == contour[currentIndex].pt1.position || i.pt2.position == contour[currentIndex].pt2.position))
            ||
            (i.pt2.position == point.Key.position && (i.pt1.position == contour[currentIndex].pt1.position || i.pt1.position == contour[currentIndex].pt2.position))
        );

        // Поиск смежного ребра.
        if (checkIndex1 != -1)
        {
            // Присвоение найденному ребру типа "Мертвое".
            contour[checkIndex1].type = Edge.EdgeType.dead;

            // Поиск второго смежного ребра.
            checkIndex2 = contour.FindIndex(i =>
                ((i.pt1.position == point.Key.position && (i.pt2.position == contour[currentIndex].pt1.position || i.pt2.position == contour[currentIndex].pt2.position))
                ||
                (i.pt2.position == point.Key.position && (i.pt1.position == contour[currentIndex].pt1.position || i.pt1.position == contour[currentIndex].pt2.position))
                &&
                i.type != Edge.EdgeType.dead)
            );

            // Второе ребро найдено => присвоение найденному ребру типа "Мертвое".
            if (checkIndex2 != -1)
                contour[checkIndex2].type = Edge.EdgeType.dead;
            // Второе ребро не найдено => добавление второго ребра и присвоение ему типа "Спящее".
            else
            {
                if (contour[checkIndex1].pt1.position == contour[currentIndex].pt1.position || contour[checkIndex1].pt2.position == contour[currentIndex].pt1.position)
                    contour.Add(new Edge(contour[currentIndex].pt2, point.Key, Edge.EdgeType.slept));
                else
                    contour.Add(new Edge(contour[currentIndex].pt1, point.Key, Edge.EdgeType.slept));
            }
        }
        // Ни одно ребро не найдено => присвоение им типа "Спящее".
        else
        {
            contour.Add(new Edge(contour[currentIndex].pt1, point.Key, Edge.EdgeType.slept));
            contour.Add(new Edge(contour[currentIndex].pt2, point.Key, Edge.EdgeType.slept));
        }

        // Если все ребра, которым принадлежит точка "мертвые", то производится удаление этой точки.
        if (!contour.All(i => i.type != Edge.EdgeType.dead && (contour[currentIndex].pt1.position == i.pt1.position || contour[currentIndex].pt1.position == i.pt2.position)))
            points.RemoveAll(i => i.position == contour[currentIndex].pt1.position);

        if (!contour.All(i => i.type != Edge.EdgeType.dead && (contour[currentIndex].pt2.position == i.pt1.position || contour[currentIndex].pt2.position == i.pt2.position)))
            points.RemoveAll(i => i.position == contour[currentIndex].pt2.position);

        if (!contour.All(i => i.type != Edge.EdgeType.dead && (point.Key.position == i.pt1.position || point.Key.position == i.pt2.position)))
            points.RemoveAll(i => i.position == point.Key.position);

        // Строим треугольник.
        triangles.Add(new Triangle(contour[currentIndex].pt1, contour[currentIndex].pt2, point.Key));
    }

    return triangles;
}

private double GetCircleRadius(Edge edge, Point3D pt)
{
    double a = (edge.pt1 - pt).position.Magnitude;
    double b = (edge.pt2 - pt).position.Magnitude;
    double c = (edge.pt1 - edge.pt2).position.Magnitude;
    double p = 0.5 * (a + b + c);

    double r = (a * b * c) / (4 * Math.Sqrt(p * (p - a) * (p - b) * (p - c)));

    return r;
}*/