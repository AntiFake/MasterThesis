using MasterProject.Agent;
using System.Collections.Generic;
using System.Linq;
using MasterProject.Core;

public class GeneralApproximation
{
    /// <summary>
    /// Исключение точек лежащих на одной прямой.
    /// </summary>
    /// <param name="agent">Агент.</param>
    private void MinimizeOnLinePoints(Agent agent)
    {
        List<int> keys = agent.observedPoints.Keys.ToList();
        // key1 - левая точка.
        // key2 - средняя точка.
        // key3 - правая точка.
        int key1, key2, key3;

        for (int i = 1; i < keys.Count - 1; i++)
        {
            key1 = keys[i - 1];
            key2 = keys[i];
            key3 = keys[i + 1];

            // Если в проверяемых наборах по 1 точке, проверяется принадлежность средней точки прямой, проведенной через крайние.
            if (agent.observedPoints[key1].Count == 1
                && agent.observedPoints[key2].Count == 1
                && agent.observedPoints[key2][0].type != Point3DType.keyPt
                && agent.observedPoints[key3].Count == 1
                && GeneralGeometry.CheckByVectorsIfPointBelongsTo3DLine(
                        (Int3)agent.observedPoints[key1][0],
                        (Int3)agent.observedPoints[key2][0],
                        (Int3)agent.observedPoints[key3][0],
                        agent.error
                    )
                )
                agent.observedPoints[key2][0].type = Point3DType.extraPt;
        }

        // Минимизирование наборов точек.
        agent.observedPoints = agent.observedPoints
            .Where
            (
                i => !(i.Value.Count == 1 && i.Value[0].type == Point3DType.extraPt)
            )
            .ToDictionary(i => i.Key, i => i.Value);
    }

    #region Минимизация точек границы области видимости.
    /// <summary>
    /// Количество итераций для алгоритма минимизации точек границ области видимости.
    /// </summary>
    /// <param name="agent"></param>
    /// <returns></returns>
    private int GetIterationsCount(Agent agent)
    {
        int limit = 0;

        // <= 1 - 4
        // > 1 && <= 3 - 3
        // > 3 && <= 5 - 2
        // > 5 && <= 10 - 1
        // > 10 - 0

        if (agent.turnOYAngle <= 1)
            limit = 4;
        if (agent.turnOYAngle > 1 && agent.turnOYAngle <= 3)
            limit = 3;
        if (agent.turnOYAngle > 3 && agent.turnOYAngle <= 5)
            limit = 2;
        if (agent.turnOYAngle > 5 && agent.turnOYAngle <= 10)
            limit = 1;

        return limit;
    }

    /// <summary>
    /// Минимизация центральной из трех точек на границе области видимости, если это возможно.
    /// </summary>
    /// <param name="agent">Агент.</param>
    /// <param name="keys">Ключи словаря точек.</param>
    /// <param name="pt_1">Точка 1.</param>
    /// <param name="pt_2">Точка 2.</param>
    /// <param name="pt_3">Точка 3.</param>
    private void CheckCirclePoints(Agent agent, List<int> keys, int pt_1, int pt_2, int pt_3)
    {
        if (agent.observedPoints[keys[pt_1]].Count == 1 && agent.observedPoints[keys[pt_2]].Count == 1 && agent.observedPoints[keys[pt_3]].Count == 1)
        {
            if (agent.observedPoints[keys[pt_1]][0].type == Point3DType.keyPt
                && agent.observedPoints[keys[pt_2]][0].type == Point3DType.keyPt
                && agent.observedPoints[keys[pt_3]][0].type == Point3DType.keyPt)
            {
                if (string.IsNullOrEmpty(agent.observedPoints[keys[pt_1]][0].obstacleName)
                    && string.IsNullOrEmpty(agent.observedPoints[keys[pt_2]][0].obstacleName)
                    && string.IsNullOrEmpty(agent.observedPoints[keys[pt_3]][0].obstacleName)
                    )
                    agent.observedPoints[keys[pt_2]][0].type = Point3DType.extraPt;
            }
        }
    }

    /// <summary>
    /// Минимизация граничных точек окружности области видимости. 
    /// </summary>
    /// <param name="agent"></param>
    private void MinimizeViewCirclePoints(Agent agent)
    {
        List<int> keys;
        // Количество итераций алгоритма минимизации.
        int limit = GetIterationsCount(agent),
            i = 0, j;

        while (i < limit)
        {
            j = 1;
            keys = agent.observedPoints.Keys.ToList();
            do
            {
                CheckCirclePoints(agent, keys, j - 1, j, j + 1);
                j++;
            } while (j < keys.Count - 1);
            // Минимизация 2 последних и первой точек.
            CheckCirclePoints(agent, keys, keys.Count - 2, keys.Count - 1, 0);

            // Минимизация последней и двух первых точек.
            CheckCirclePoints(agent, keys, keys.Count - 1, 0, 1);

            // Исключение лишних точек.
            agent.observedPoints = agent.observedPoints
                .Where(n => !(n.Value.Count == 1 && n.Value[0].type == Point3DType.extraPt))
                .ToDictionary(n => n.Key, n => n.Value);

            i++;
        }
    }
    #endregion

    /// <summary>
    /// Аппроксимация точек.
    /// </summary>
    /// <param name="agent">Агент.</param>
    public void ApproximatePoints(Agent agent)
    {
        MinimizeOnLinePoints(agent);
        MinimizeViewCirclePoints(agent);
    }
}
