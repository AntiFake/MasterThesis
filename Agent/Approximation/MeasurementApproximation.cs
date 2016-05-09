using MasterProject.Core;
using System.Collections.Generic;
using System.Linq;

namespace MasterProject.Agent.Approximation
{
    /// <summary>
    /// Класс, реализующий методы аппроксимации точек для наборов словаря измерений.
    /// </summary>
    public class MeasurementApproximation
    {
        /// <summary>
        /// Удаление одинаковых точек. Сортировка точек набора по Oy.
        /// </summary>
        /// <param name="pointsSet">Набор точек.</param>
        /// <returns></returns>
        private List<Point3D> DeleteDuplicatePoints(List<Point3D> pointsSet)
        {
            return pointsSet.Distinct(new Point3DEqualityComparer())
                        .OrderBy(i => i.position.y)
                        .Select((i, index) =>
                        {
                            i.index_y = index;
                            return i;
                        })
                        .ToList();
        }

        /// <summary>
        /// Поиск минимального по длине вектора.
        /// </summary>
        /// <param name="vectors">Коллекция векторов.</param>
        /// <returns></returns>
        private int FindMinMagnitudeVector(List<KeyValuePair<string, double>> vectors)
        {
            double minMagnitude = vectors[0].Value;
            int minIndex = 0;

            for (int i = 1; i < vectors.Count; i++)
            {
                if (minMagnitude > vectors[i].Value)
                {
                    minMagnitude = vectors[i].Value;
                    minIndex = i;
                }
            }

            return minIndex;
        }

        /// <summary>
        /// Получение точек, принадлежащих близжайшему объекту. 
        /// В случае если в наборе точки принадлежат разным объектам.
        /// </summary>
        /// <param name="pointsSet">Набор точек.</param>
        /// <returns></returns>
        private List<Point3D> GetClosestObjectPoints(Agent agent, List<Point3D> pointsSet)
        {
            // 1. Длины векторов (начало - нижний сканер, конец - точка набора).
            List<KeyValuePair<string, double>> vectors = pointsSet
                .Select
                (
                    i => new KeyValuePair<string, double>(i.obstacleName, ((Int2)(i.position - agent.groundScannerInt3Pos)).Magnitude)
                )
                .ToList();

            // 2. Находим вектор минимальной длины.
            int minIndex = FindMinMagnitudeVector(vectors);

            // 3. Если длина найденного вектора < длины первого вектора, то в наборе оставляем единственную точку и опускаем ее на уровень сканера "Земля".
            if (vectors[minIndex].Value < vectors[0].Value)
                return new List<Point3D>()
            {
                new Point3D
                (
                    new Int3(pointsSet[minIndex].position.x, agent.groundScannerInt3Pos.y, pointsSet[minIndex].position.z),
                    pointsSet[minIndex].obstacleName,
                    pointsSet[minIndex].type
                )
            };

            // 4. Возвращаем точки, принадлежащие объекту, расстояние до которого минимальное.
            return pointsSet.Where(i => i.obstacleName == vectors[minIndex].Key || string.IsNullOrEmpty(i.obstacleName)).ToList();
        }

        /// <summary>
        /// Минимизация точек, лежащих на одной прямой.
        /// </summary>
        /// <param name="pointsSet">Набор точек.</param>
        /// <returns></returns>
        private List<Point3D> MinimizeOnLinePoints(Agent agent, List<Point3D> pointsSet)
        {
            // Итерации цикла производятся с 2-ой точки набора по предпоследнюю.
            // Каждая центральная точка в поднаборе из 3-х точек проверяется на условие коллинеарности.
            // Если ц. точка лежит на одной прямой с двумя крайними точками поднабора, то точка считается избыточной.
            for (int i = 1; i < pointsSet.Count - 1; i++)
            {
                if (GeneralGeometry.IsPointBelongsTo3DLine((Int3)pointsSet[i - 1], (Int3)pointsSet[i], (Int3)pointsSet[i + 1], agent.error))
                    pointsSet[i].type = Point3DType.extraPt;
            }

            return pointsSet.Where(i => i.type != Point3DType.extraPt).ToList();
        }

        /// <summary>
        /// Обработка набора, состоящего из единственной точки.
        /// </summary>
        /// <param name="pointsSet">Набор точек.</param>
        /// <returns></returns>
        private List<Point3D> HandleSinglePoint(Agent agent, List<Point3D> pointsSet)
        {
            if (string.IsNullOrEmpty(pointsSet[0].obstacleName))
                pointsSet[0].type = Point3DType.keyPt;
            else
                pointsSet[0].type = Point3DType.sealedPt;

            pointsSet[0].position.y = agent.groundScannerInt3Pos.y;

            return pointsSet;
        }


        /// <summary>
        /// Аппроксимация точек кривых выпуклых/вогнутых поверхностей.
        /// </summary>
        /// <param name="pointsSet">Набор точек.</param>
        /// <returns></returns>
        private List<Point3D> CheckBumpness(Agent agent, List<Point3D> pointsSet)
        {
            List<Point3D> resultSet = new List<Point3D>();

            // 1. Словарь. Индекс точки - длина вектора (от сканера "Земля" до точки набора).
            Dictionary<int, double> vectors = new Dictionary<int, double>();
            foreach (var point in pointsSet)
            {
                vectors.Add(point.index_y, ((Int2)(point.position - agent.groundScannerInt3Pos)).Magnitude);
            }

            // 2. Сортируем словарь по длинам векторов.
            var list = vectors
                            .OrderBy(i => i.Value)
                            .Select(i => new { index = i.Key, magnitude = i.Value })
                            .ToList();

            // 3. Производим сравнение.
            Point3D tmpPt = null;

            for (int i = 0; i < vectors.Count; i++)
            {
                // Индексы одинаковые - добавляем точку в набор. 
                if (pointsSet[i].index_y == list[i].index)
                {
                    pointsSet[i].type = Point3DType.keyPt;
                    resultSet.Add(pointsSet[i]);
                }
                // Индексы не одинаковые - выходим из цикла.
                else
                {
                    // Первая точка.
                    if (i == 0)
                    {
                        tmpPt = pointsSet.First(n => n.index_y == list[i].index);
                        tmpPt.position.y = agent.groundScannerInt3Pos.y;
                        tmpPt.type = Point3DType.sealedPt;
                        resultSet.Add(tmpPt);
                        break;
                    }
                    else
                    {
                        resultSet[i - 1].type = Point3DType.sealedPt;
                        break;
                    }
                }
            }

            return resultSet;
        }

        /// <summary>
        /// Обработка наклонных плоскостей.
        /// </summary>
        /// <param name="agent">Агент.</param>
        /// <param name="pointsSet">Набор точек.</param>
        /// <returns></returns>
        private List<Point3D> HandleSlopes(Agent agent, List<Point3D> pointsSet)
        {
            List<Point3D> result = new List<Point3D>();
            result.Add(pointsSet[0]);
            Int3 vector1, vector2, projection;

            for (int i = 0; i < pointsSet.Count - 1; i++)
            {
                // 1. Находим вектора.
                // Между точками наклонной плоскости.
                vector1 = new Int3(pointsSet[i + 1].position - pointsSet[i].position);
                // Между i-ой точкой и проекцией i+1 точки на горизонталь из i-ой точки.
                projection = new Int3(pointsSet[i + 1].position.x, pointsSet[i].position.y, pointsSet[i + 1].position.z);

                vector2 = new Int3(projection - pointsSet[i].position);

                // 2. Проверка на проходимость.
                if (vector1.GetAngle(vector2) < agent.maxSlopeAngle)
                    result.Add(pointsSet[i + 1]);
                else
                {
                    result[i].type = Point3DType.sealedPt;
                    break;
                }
            }

            return result;
        }

        /// <summary>
        /// Минимизация точек в наборах.
        /// </summary>
        /// <param name="agent">Агент.</param>
        public void MinimizeObservedPoints(Agent agent)
        {
            if (agent.observedPoints.Any())
            {
                List<int> keys = agent.observedPoints.Keys.ToList();
                foreach (int key in keys)
                {
                    List<Point3D> pointsSet = agent.observedPoints[key];
                    switch (pointsSet.Count)
                    {
                        // 0 точек в наборе.
                        case 0:
                            break;
                        // 1 точка в наборе.
                        case 1:
                            agent.observedPoints[key] = HandleSinglePoint(agent, pointsSet);
                            break;
                        // 2 и более точек. 
                        default:
                            pointsSet = DeleteDuplicatePoints(pointsSet);
                            pointsSet = MinimizeOnLinePoints(agent, pointsSet);
                            pointsSet = GetClosestObjectPoints(agent, pointsSet);

                            // Если после первых трех стадий обработки в словаре остается 1-а точка.
                            if (pointsSet.Count == 1)
                            {
                                agent.observedPoints[key] = HandleSinglePoint(agent, pointsSet);
                                break;
                            }

                            pointsSet = CheckBumpness(agent, pointsSet);
                            agent.observedPoints[key] = HandleSlopes(agent, pointsSet);

                            break;
                    }
                }
            }
        }
    }
}