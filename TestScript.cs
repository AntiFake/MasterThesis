// Translate, rotate and scale a mesh. Try varying
// the parameters in the inspector while running
// to see the effect they have.

using UnityEngine;
using System.Collections;

public class TestScript : MonoBehaviour
{
    public Transform tr;


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