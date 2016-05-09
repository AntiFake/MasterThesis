using MasterProject.Core;
using System.Collections.Generic;
using System.Linq;

namespace MasterProject.Agent.ContourBuilder
{
    public class OutlineBuilder
    {
        /// <summary>
        /// Получение основного контура.
        /// </summary>
        /// <param name="agent">Агент.</param>
        /// <param name="outlines">Точки контуров.</param>
        /// <returns></returns>
        private void GetGroundOutline(Agent agent, ref List<List<Point3D>> outlines)
        {
            outlines.Add(agent.observedPoints.Select(i => i.Value[0]).ToList());
        }

        /// <summary>
        /// Получение точек контура части наклонной поверхности.
        /// </summary>
        /// <param name="agent">Агент.</param>
        /// <param name="outlines">Точки контуров.</param>
        /// <param name="key1">Набор 1.</param>
        /// <param name="key2">Набор 2.</param>
        private void GetSlopePartOutline(Agent agent, ref List<List<Point3D>> outlines, int key1, int key2)
        {
            if (agent.observedPoints[key1].Count > 1 && agent.observedPoints[key2].Count > 1)
            {
                outlines.Add(new List<Point3D>());
                outlines[outlines.Count - 1].AddRange(agent.observedPoints[key1]);

                for (int j = agent.observedPoints[key2].Count - 1; j >= 0; j--)
                {
                    outlines[outlines.Count - 1].Add(agent.observedPoints[key2][j]);
                }
            }
        }

        /// <summary>
        /// Получение точек контуров частей наклонной плоскости.
        /// </summary>
        /// <param name="agent">Агент.</param>
        /// <param name="outlines">Точки контуров.</param>
        private void GetSlopePartsOutline(Agent agent, ref List<List<Point3D>> outlines)
        {
            int[] keys = agent.observedPoints.Keys.ToArray();

            for (int i = 0; i < keys.Length - 1; i++)
            {
                GetSlopePartOutline(agent, ref outlines, keys[i], keys[i + 1]);
            }

            GetSlopePartOutline(agent, ref outlines, keys[keys.Length - 1], keys[0]);
        }

        /// <summary>
        /// Формирование точек контуров "земли" и наклонных плоскостей.
        /// </summary>
        /// <param name="agent"></param>
        /// <returns></returns>
        public List<List<Point3D>> GetOutlines(Agent agent)
        {
            List<List<Point3D>> outlines = new List<List<Point3D>>();

            GetGroundOutline(agent, ref outlines);
            GetSlopePartsOutline(agent, ref outlines);

            return outlines;
        }

        public void ApproximateOutlines(ref List<List<Point3D>> outlines)
        {
            //for(int i = 0; i < out)
        }
    }
}