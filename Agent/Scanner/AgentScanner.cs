using UnityEngine;
using System.Collections.Generic;
using MasterProject.Core;

namespace MasterProject.Agent.Scanner
{
    /// <summary>
    /// Класс, описывающий основные методы и функции для сканера.
    /// </summary>
    public class AgentScanner
    {
        /// <summary>
        /// Буфер, в который сохраняется набор точек одного измерения сканера.
        /// </summary>
        protected List<Point3D> ptsBuffer;

        public AgentScanner()
        {
            ptsBuffer = new List<Point3D>();
        }

        /// <summary>
        /// Запуск лучей сканера.
        /// </summary>
        /// <param name="agent">Агент</param>
        /// <param name="angle">Текущий угол поворота сканера</param>
        public virtual void CastRays(Agent agent, int angle) { }

        /// <summary>
        /// Поворот сканера.
        /// </summary>
        /// <param name="agent">Агент.</param>
        /// <param name="direction">Направление. Если = 1, то по часовой стрелке, если = -1, то против.</param>
        public virtual void Rotate(Agent agent, int direction = 1) { }

        /// <summary>
        /// Сброс угла поворота сканера.
        /// </summary>
        /// <param name="agent">Агент.</param>
        public virtual void ResetRotation(Agent agent) { }

        /// <summary>
        /// Запуск одного луча сканера.
        /// </summary>
        /// <param name="agent">Агента.</param>
        /// <param name="rayOrigin">Точка, из которой производится пуск луча.</param>
        /// <param name="rayDirection">Направление, в котором производится пуск луча.</param>
        protected void CastRay(Agent agent, Vector3 rayOrigin, Vector3 rayDirection)
        {
            RaycastHit hit;
            Ray ray = new Ray(rayOrigin, rayDirection);

            if (Physics.Raycast(ray, out hit, agent.rayLength, agent.layerObstacles) && hit.point != null)
                ptsBuffer.Add(new Point3D((Int3)hit.point, hit.collider.name));
        }

        /// <summary>
        /// Момент начала сканирования. Создание элемента словаря с набором точек.
        /// </summary>
        /// <param name="agent">Агент.</param>
        /// <param name="angle">Текущий угол поворота.</param>
        protected void BeginScan(Agent agent, int angle)
        {
            if (!agent.observedPoints.ContainsKey(angle))
                agent.observedPoints.Add(angle, new List<Point3D>());
        }

        /// <summary>
        /// Момент завершения сканирования. Сохранение набора полученных точек. Очистка временного буфера.
        /// </summary>
        /// <param name="agent">Агент.</param>
        /// <param name="angle">Текущий угол поворота.</param>
        protected void EndScan(Agent agent, int angle)
        {
            agent.observedPoints[angle].AddRange(ptsBuffer);
            ptsBuffer.Clear();
        }
    }
}