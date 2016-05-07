using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using MasterProject.Core;
using MasterProject.VisualDebug;
using MasterProject.NavMesh;
using MasterProject.Agent.Scanner;

namespace MasterProject.Agent
{
    public partial class Agent : MonoBehaviour
    {
        [Header("Маска для слоя препятствий")]
        public LayerMask layerObstacles;

        [Header("Радиус области видимости")]
        public int rayLength = 20;

        [Header("Угол смещения сканера")]
        public int turnOYAngle = 30;

        [Header("Максимально допустимый угол подъема")]
        public float maxSlopeAngle = 30f;

        [Header("Допустимый уровень погрешности")]
        public float error = 3;

        [Header("Верхний сканер")]
        public Transform headScannerTransform;

        [Header("Нижний сканер")]
        public Transform groundScannerTransform;

        private AgentHeadScanner headScanner;
        private AgentGroundScanner groundScanner;

        // Словарь для хранения найденных точек.
        // Градус отклонения лучей - ключ, массив точек - список значений.
        [HideInInspector]
        public Dictionary<int, List<Point3D>> observedPoints;
        
        // Механизм разбиения проходимой области на треугольники.
        private List<Contour> contours;
        private Contour c;
        private Triangulator triangulator;
        private List<Triangle> passableArea;

        #region Сканеры
        private Int3 headScannerInt3Pos;
        private Int3 groundScannerInt3Pos;
        #endregion
        
        #region MonoBehavior-функции
        public void Awake()
        {
            raysDebug = new List<RayDebug>();
            navMeshDebug = new NavMeshDebug();
            triangulator = new Triangulator();

            observedPoints = new Dictionary<int, List<Point3D>>();
            passableArea = new List<Triangle>();
            contours = new List<Contour>();

            headScanner = new AgentHeadScanner();
            groundScanner = new AgentGroundScanner();
        }

        public void Start()
        {
            ScanArea();
        }
        #endregion

        #region Получение информации об области вокруг
        /// <summary>
        /// Сканирование области на 360 градусов вокруг.
        /// </summary>
        private void ScanArea()
        {
            // Сохраняем позиции сканеров в Int3 для последующих расчетов.
            headScannerInt3Pos = (Int3)headScannerTransform.position;
            groundScannerInt3Pos = (Int3)groundScannerTransform.position;

            int fullCircle = 360, i = (int)headScannerTransform.eulerAngles.y;

            while (i < fullCircle)
            {
                groundScanner.CastRays(this, i);
                headScanner.CastRays(this, i);
                groundScanner.Rotate(this);
                headScanner.Rotate(this);

                i += turnOYAngle;
            }

            groundScanner.ResetRotation(this);
            headScanner.ResetRotation(this);
        }
        #endregion

        #region Обработка полученных точек карты

        /// <summary>
        /// Удаление точек-дубликатов + сортировка по Oy.
        /// </summary>
        /// <param name="points">Набор точек</param>
        /// <returns></returns>
        private List<Point3D> DeleteDuplicatePoints(List<Point3D> points)
        {
            return points.Distinct(new Point3DEqualityComparer())
                            .OrderBy(i => i.position.y)
                            .Select((i, index) =>
                            {
                                i.index_y = index;
                                return i;
                            })
                            .ToList();
        }

        /// <summary>
        /// Удаление точек, не принадлежащих ближайшему объекту.
        /// </summary>
        /// <param name="points"></param>
        /// <returns></returns>
        /// 1. Бежим по набору точек.
        /// 2. Смотрим если точка принадлежит одному и тому же объекту - продолжаем движение.
        /// 3. Если внезапно принадлежит другому, то сохраняем ее и делаем break.
        private List<Point3D> DeleteExtraObjectPoints(List<Point3D> points)
        {
            List<KeyValuePair<string, double>> vectors = points
                .Select(i => new KeyValuePair<string, double>(i.obstacleName, ((Int2)(i.position - groundScannerInt3Pos)).Magnitude))
                .ToList();

            string on = vectors[0].Key;
            double min = vectors[0].Value;
            int index = 0;

            for (int i = 1; i < vectors.Count; i++)
            {
                if (min > vectors[i].Value)
                {
                    min = vectors[i].Value;
                    on = vectors[i].Key;
                    index = i;
                }
            }

            // Исключение висячих точек в воздухе.
            if (min < vectors[0].Value)
            {
                return new List<Point3D>()
                {
                    new Point3D(new Int3(points[index].position.x, ((Int3)groundScannerTransform.position).y, points[index].position.z), points[index].obstacleName, points[index].type)
                };
            }

            return points.Where(i => i.obstacleName == on || string.IsNullOrEmpty(i.obstacleName)).ToList();
        }

        /// <summary>
        /// Удаление лишних точек, лежащих на одной прямой
        /// </summary>
        /// <param name="points"></param>
        /// <returns></returns>
        private List<Point3D> DeleteOnlineExtraPoints(List<Point3D> points)
        {
            // Итерации цикла производятся с 2-ой точки набора по предпоследнюю.
            // Каждая центральная точка в поднаборе из 3-х точек проверяется на условие коллинеарности.
            // Если ц. точка лежит на одной прямой с двумя крайними точками поднабора, то точка считается избыточной.
            for (int i = 1; i < points.Count - 1; i++)
            {
                if (GeneralGeometry.CheckByVectorsIfPointBelongsTo3DLine((Int3)points[i - 1], (Int3)points[i], (Int3)points[i + 1], error))
                {
                    points[i].type = Point3DType.extraPt;
                }
            }

            return points.Where(i => i.type != Point3DType.extraPt).ToList();
        }

        /// <summary>
        /// Случай, если набор точек представляет собой последовательность из одного элемента
        /// </summary>
        /// <param name="point">Точка</param>
        /// <returns></returns>
        public Point3D HandleSinglePoint(Point3D point)
        {
            if (string.IsNullOrEmpty(point.obstacleName))
                point.type = Point3DType.keyPt;
            else
                point.type = Point3DType.sealedPt;

            // TODO: впоследствии заменить на уровень земли.
            point.position.y = groundScannerInt3Pos.y;

            return point;
        }

        /// <summary>
        /// Аппроксимация точек кривых выпуклых/вогнутых поверхностей.
        /// </summary>
        /// <param name="points"></param>
        /// <returns></returns>
        public List<Point3D> CheckBumpness(List<Point3D> points)
        {
            List<Point3D> result = new List<Point3D>();

            // Словарь для хранения индекса точки - длины вектора
            Dictionary<int, double> vectors = new Dictionary<int, double>();

            // Формируем словарь.
            foreach (var point in points)
            {
                vectors.Add(point.index_y, ((Int2)(point.position - groundScannerInt3Pos)).Magnitude);
            }
            var list = vectors.OrderBy(i => i.Value)
                                .Select(i => new { index = i.Key, magnitude = i.Value })
                                .ToList();

            // Производим сравнение.
            Point3D tmpPt = null;

            for (int i = 0; i < vectors.Count; i++)
            {
                if (points[i].index_y == list[i].index)
                {
                    points[i].type = Point3DType.keyPt;
                    result.Add(points[i]);
                }
                else
                {
                    // Первая точка.
                    if (i == 0)
                    {
                        tmpPt = points.First(n => n.index_y == list[i].index);
                        tmpPt.position.y = groundScannerInt3Pos.y;
                        tmpPt.type = Point3DType.sealedPt;
                        result.Add(tmpPt);
                        break;
                    }
                    else
                    {
                        result[i - 1].type = Point3DType.sealedPt;
                        break;
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Проверка наклонных плоскостей на проходимость.
        /// </summary>
        private List<Point3D> HandleSlopes(List<Point3D> points)
        {
            List<Point3D> result = new List<Point3D>();
            result.Add(points[0]);
            Int3 vector1, vector2, projection;

            for (int i = 0; i < points.Count - 1; i++)
            {
                // 1. Находим вектора
                // Между точками наклонной плоскости.
                vector1 = new Int3(points[i + 1].position - points[i].position);
                // Между i-ой точкой и проекцией i+1 точки на горизонталь из i-ой точки.
                projection = new Int3(points[i + 1].position.x, points[i].position.y, points[i + 1].position.z);
                vector2 = new Int3(projection - points[i].position);

                // 2. Поиск величины угла
                if (vector1.GetAngle(vector2) < maxSlopeAngle)
                    result.Add(points[i + 1]);
                else
                {
                    result[i].type = Point3DType.sealedPt;
                    break;
                }
            }

            return result;
        }

        /// <summary>
        /// Удаление лишних точек.
        /// </summary>
        private void ExcludeExtraPts()
        {
            if (observedPoints.Any())
            {
                List<int> keys = observedPoints.Keys.ToList();
                foreach (int key in keys)
                {
                    List<Point3D> ptsSet = observedPoints[key];
                    switch (ptsSet.Count)
                    {
                        // 0 точек в наборе.
                        case 0:
                            break;
                        // 1 точка в наборе. Делаем эту точку ключевой.
                        case 1:
                            ptsSet[0] = HandleSinglePoint(ptsSet[0]);
                            observedPoints[key] = ptsSet;
                            break;
                        // 2 и более точек. Выделение избыточных точек. 
                        default:
                            ptsSet = DeleteDuplicatePoints(ptsSet);
                            ptsSet = DeleteOnlineExtraPoints(ptsSet);
                            ptsSet = DeleteExtraObjectPoints(ptsSet);

                            // Если после первых трех стадий обработки в словаре остается 1-а точка.
                            if (ptsSet.Count == 1)
                            {
                                ptsSet[0] = HandleSinglePoint(ptsSet[0]);
                                observedPoints[key] = ptsSet;
                                break;
                            }

                            ptsSet = CheckBumpness(ptsSet);
                            observedPoints[key] = HandleSlopes(ptsSet);

                            break;
                    }
                }
            }
        }

        /// <summary>
        /// Аппроксимация точек. 
        /// </summary>
        private void ApproximatePoints()
        {
            if (observedPoints.Any())
            {
                List<int> keys = observedPoints.Keys.ToList();
                int key1, key2, key3, j;

                for (int i = 1; i < keys.Count - 1; i++)
                {
                    key1 = keys[i - 1];
                    key2 = keys[i];
                    key3 = keys[i + 1];

                    // TODO: дописать условие на принадлежность точки объекту.
                    if (observedPoints[key1].Count == 1 
                        && observedPoints[key2].Count == 1 
                        && observedPoints[key2][0].type != Point3DType.keyPt 
                        && observedPoints[key3].Count == 1 
                        && GeneralGeometry.CheckByVectorsIfPointBelongsTo3DLine((Int3)observedPoints[key1][0], (Int3)observedPoints[key2][0], (Int3)observedPoints[key3][0], error)
                        )
                    {
                        observedPoints[key2][0].type = Point3DType.extraPt;
                    }
                }

                observedPoints = observedPoints.Where(i => !(i.Value.Count == 1 && i.Value[0].type == Point3DType.extraPt)).ToDictionary(i => i.Key, i => i.Value);

                // <= 1 - 4
                // > 1 && <= 3 - 3
                // > 3 && <= 5 - 2
                // > 5 && <= 10 - 1
                // > 10 - 0

                int limit = 0;
                int a = 0;

                if (turnOYAngle <= 1)
                    limit = 4;
                if (turnOYAngle > 1 && turnOYAngle <= 3)
                    limit = 3;
                if (turnOYAngle > 3 && turnOYAngle <= 5)
                    limit = 2;
                if (turnOYAngle > 5 && turnOYAngle <= 10)
                    limit = 1;

                while (a < limit)
                {
                    j = 1;
                    keys = observedPoints.Keys.ToList();
                    do
                    {
                        f(keys, j - 1, j, j + 1);
                        j++;
                    } while (j < keys.Count - 1);
                    f(keys, keys.Count - 2, keys.Count - 1, 0);
                    f(keys, keys.Count - 1, 0, 1);

                    observedPoints = observedPoints.Where(i => !(i.Value.Count == 1 && i.Value[0].type == Point3DType.extraPt)).ToDictionary(i => i.Key, i => i.Value);
                    a++;
                }
            }
        }

        private void f(List<int> keys, int j, int n, int m)
        {
            if (observedPoints[keys[j]].Count == 1 && observedPoints[keys[n]].Count == 1 && observedPoints[keys[m]].Count == 1)
            {
                if (observedPoints[keys[j]][0].type == Point3DType.keyPt
                    && observedPoints[keys[n]][0].type == Point3DType.keyPt
                    && observedPoints[keys[m]][0].type == Point3DType.keyPt)
                {
                    if (string.IsNullOrEmpty(observedPoints[keys[j]][0].obstacleName)
                        && string.IsNullOrEmpty(observedPoints[keys[n]][0].obstacleName)
                        && string.IsNullOrEmpty(observedPoints[keys[m]][0].obstacleName))
                    {
                        observedPoints[keys[n]][0].type = Point3DType.extraPt;
                    }
                }
            }
        }

        public List<List<Point3D>> GetLayers()
        {
            List<List<Point3D>> layers = new List<List<Point3D>>();

            // 1. Уровень земли (где число точек 1).
            layers.Add(observedPoints.Select(i => i.Value[0]).ToList());

            // 2. Наклонные плоскости (где число точек > 2).
            int[] keys = observedPoints.Keys.ToArray();

            for (int i = 0; i < keys.Length - 1; i++)
            {
                if (observedPoints[keys[i]].Count > 1 && observedPoints[keys[i + 1]].Count > 1)
                {
                    layers.Add(new List<Point3D>());
                    layers[layers.Count - 1].AddRange(observedPoints[keys[i]]);

                    for (int j = observedPoints[keys[i + 1]].Count - 1; j >= 0; j--)
                    {
                        layers[layers.Count - 1].Add(observedPoints[keys[i + 1]][j]);
                    }
                }
            }

            if (observedPoints[keys[keys.Length - 1]].Count > 1 && observedPoints[keys[0]].Count > 1)
            {
                layers.Add(new List<Point3D>());
                layers[layers.Count - 1].AddRange(observedPoints[keys[keys.Length - 1]]);

                for (int j = observedPoints[keys[0]].Count - 1; j >= 0; j--)
                {
                    layers[layers.Count - 1].Add(observedPoints[keys[0]][j]);
                }
            }

            return layers;
        }

        #endregion
    }
}