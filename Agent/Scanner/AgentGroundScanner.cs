using MasterProject.Core;
using UnityEngine;

namespace MasterProject.Agent.Scanner
{
    /// <summary>
    /// Класс, реализующий сканер, находящийся на уровне земли ("Земля").
    /// </summary>
    public class AgentGroundScanner : AgentScanner
    {
        /// <summary>
        /// Запуск лучей сканера "Земля".
        /// </summary>
        /// <param name="agent">Агент.</param>
        /// <param name="angle">Текущий угол поворота.</param>
        public override void CastRays(Agent agent, int angle)
        {
            Vector3 dirOX = agent.groundScannerTransform.right * agent.rayLength,
                    dirOY,
                    agentPos = agent.transform.position,
                    headPos = agent.headScannerTransform.position,
                    groundPos = agent.groundScannerTransform.position;

            BeginScan(agent, angle);

            // Ground-to-Ground.
            CastGroundRay(agent, groundPos, dirOX);

            // Ground-to-Middle.
            dirOY = agentPos - groundPos;
            CastRay(agent, groundPos, dirOX + dirOY);

            //Ground-to-Head.
            dirOY = headPos - groundPos;
            CastRay(agent, groundPos, dirOX + dirOY);

            EndScan(agent, angle);
        }

        /// <summary>
        /// Запуск луча на уровне земли для сохранение точек области видимости.
        /// </summary>
        /// <param name="agent">Агент.</param>
        /// <param name="rayOrigin">Точка начала луча.</param>
        /// <param name="rayDirection">Направление луча.</param>
        private void CastGroundRay(Agent agent, Vector3 rayOrigin, Vector3 rayDirection)
        {
            RaycastHit hit;
            Ray ray = new Ray(rayOrigin, rayDirection);

            if (Physics.Raycast(ray, out hit, agent.rayLength, agent.layerObstacles) && hit.point != null)
                ptsBuffer.Add(new Point3D((Int3)hit.point, hit.collider.name));
            else
                ptsBuffer.Add((Point3D)(rayOrigin + rayDirection));
        }

        /// <summary>
        /// Сброс угла поворота сканера "Земля".
        /// </summary>
        /// <param name="agent">Агент.</param>
        public override void ResetRotation(Agent agent)
        {
            agent.groundScannerTransform.rotation = Quaternion.identity;
        }

        /// <summary>
        /// Поворот сканера "Земля".
        /// </summary>
        /// <param name="agent">Агент.</param>
        /// <param name="direction">Направление. Если = 1, то по часовой стрелке, если = -1, то против.</param>
        public override void Rotate(Agent agent, int direction = 1)
        {
            agent.groundScannerTransform.Rotate(new Vector3(0f, agent.turnOYAngle * direction, 0f));
        }
    }
}
