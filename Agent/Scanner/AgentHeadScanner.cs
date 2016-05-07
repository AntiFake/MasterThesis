using UnityEngine;

namespace MasterProject.Agent.Scanner
{
    /// <summary>
    /// Класс, реализующий сканер, находящийся на уровне головы ("Голова").
    /// </summary>
    public class AgentHeadScanner : AgentScanner
    {
        /// <summary>
        /// Запуск лучей сканера "Голова".
        /// </summary>
        /// <param name="agent">Агент.</param>
        /// <param name="angle">Текущий угол поворота.</param>
        public override void CastRays(Agent agent, int angle)
        {
            Vector3 dirOX = agent.headScannerTransform.right * agent.rayLength,
                    dirOY,
                    agentPos = agent.transform.position,
                    headPos = agent.headScannerTransform.position,
                    groundPos = agent.groundScannerTransform.position;

            BeginScan(agent, angle);

            // Head-to-Head.
            CastRay(agent, headPos, dirOX);

            // Head-to-Middle.
            dirOY = agentPos - headPos;
            CastRay(agent, headPos, dirOX + dirOY);

            // Head-to-Ground.
            dirOY = groundPos - headPos;
            CastRay(agent, headPos, dirOX + dirOY);

            EndScan(agent, angle);
        }

        /// <summary>
        /// Сброс угла поворота сканера "Голова".
        /// </summary>
        /// <param name="agent">Агент.</param>
        public override void ResetRotation(Agent agent)
        {
            agent.headScannerTransform.rotation = Quaternion.identity;
        }

        /// <summary>
        /// Поворот сканера "Голова".
        /// </summary>
        /// <param name="agent">Агент.</param>
        /// <param name="direction">Направление. Если = 1, то по часовой стрелке, если = -1, то против.</param>
        public override void Rotate(Agent agent, int direction = 1)
        {
            agent.headScannerTransform.Rotate(new Vector3(0f, agent.turnOYAngle * direction, 0f));
        }
    }
}
