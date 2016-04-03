using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using MasterProject.Core;
using MasterProject.VisualDebug;
using MasterProject.NavMesh;

namespace MasterProject.Agent
{
    public class Agent : MonoBehaviour
    {
        public LayerMask layerObstacles;
        public int rayLength = 20;
        public int turnOYAngle = 30;
        public float maxSlopeAngle = 30f;
        public float error = 3;

        // Сканеры (пустые объекты на "голове" и "под ногами" агента)
        public Transform headScanner;
        public Transform groundScanner;

        #region Лучи-детекторы
        private Ray hh_Ray;
        private Vector3 hh_Direction;

        private Ray hm_Ray;
        private Vector3 hm_Direction;

        private Ray hg_Ray;
        private Vector3 hg_Direction;

        private Ray gg_Ray;
        private Vector3 gg_Direction;

        private Ray gm_Ray;
        private Vector3 gm_Direction;

        private Ray gh_Ray;
        private Vector3 gh_Direction;

        // Словарь для хранения найденных точек.
        // Градус отклонения лучей - ключ, массив точек - список значений.
        private Dictionary<int, List<Point3D>> observedPoints;
        // Буфер для хранения точек. Значениями из буфера заполняется словарь хранения найденных точек.
        private List<Point3D> ptsBuffer;
        // Механизм разбиения проходиомй области на треугольники.
        private Triangulator triangulator;
        private List<Triangle> passableArea;
        #endregion

        #region Сканеры
        private Int3 headScannerInt3Pos;
        private Int3 groundScannerInt3Pos;
        #endregion

        #region Debug
        private List<RayDebug> raysDebug;
        private NavMeshDebug navMeshDebug;
        #endregion

        #region MonoBehavior-функции
        public void Awake()
        {
            raysDebug = new List<RayDebug>();
            navMeshDebug = new NavMeshDebug();
            triangulator = new Triangulator();

            observedPoints = new Dictionary<int, List<Point3D>>();
            ptsBuffer = new List<Point3D>();
        }

        public void Start()
        {
            ScanArea();
        }

        public void OnGUI()
        {
            if (GUI.Button(new Rect(0f, 0f, 80, 30), "Excl"))
                ExcludeExtraPts();
            if (GUI.Button(new Rect(0f, 50f, 80, 30), "Approx."))
                ApproximatePoints();
            if (GUI.Button(new Rect(0f, 150f, 80f, 30f), "test"))
                passableArea = triangulator.TriangulateArea(new Contour(observedPoints));
        }

        public void OnDrawGizmos()
        {
            // Отображение точек.
            if (observedPoints != null && observedPoints.Any())
            {
                foreach (KeyValuePair<int, List<Point3D>> ptsSet in observedPoints)
                {
                    foreach (Point3D pt in ptsSet.Value)
                    {
                        Gizmos.color = pt.PointColor;
                        Gizmos.DrawCube((Vector3)pt.position, new Vector3(0.1f, 0.1f, 0.1f));
                    }
                }
            }

            // Отображение треугольников
            if (passableArea != null)
                navMeshDebug.DrawTriangles(Color.magenta, passableArea);


            // Отображение лучей сканеров.
            if (raysDebug != null && raysDebug.Any())
            {
                foreach (RayDebug r in raysDebug)
                {
                    Debug.DrawRay(r.origin, r.direction, r.color);
                }
            }
        }

        #endregion

        #region Получение информации об области вокруг
        /// <summary>
        /// Сканирование области на 360 градусов вокруг.
        /// </summary>
        private void ScanArea()
        {
            // Сохраняем позиции сканеров в Int3 для последующих расчетов.
            headScannerInt3Pos = (Int3)headScanner.position;
            groundScannerInt3Pos = (Int3)groundScanner.position;

            int fullCircle = 360, i = (int)headScanner.eulerAngles.y;

            while (i < fullCircle)
            {
                CastDownRays(i);
                CastUpRays(i);
                RotateRayScanners();

                i += turnOYAngle;
            }

            ResetAllScanners();
        }

        /// <summary>
        /// Запуск лучей из верхнего детектора
        /// </summary>
        /// <param name="angle">Угол поворота относительно вертикали</param>
        private void CastUpRays(int angle)
        {
            // Инициализация.
            Vector3 dirOX = headScanner.right * rayLength;
            Vector3 dirOY;
            Vector3 agentPos = transform.position,
                    headPos = headScanner.position,
                    groundPos = groundScanner.position;

            if (!observedPoints.ContainsKey(angle))
            {
                observedPoints.Add(angle, new List<Point3D>());
            }

            // Head-to-Head.
            hh_Direction = dirOX;

            hh_Ray = new Ray(headPos, hh_Direction);
            RaycastHit hh_RayHit;

            if (Physics.Raycast(hh_Ray, out hh_RayHit, rayLength, layerObstacles) && hh_RayHit.point != null)
            {
                ptsBuffer.Add(new Point3D((Int3)hh_RayHit.point, hh_RayHit.collider.name));
            }

            // Head-to-Middle.
            dirOY = new Vector3(agentPos.x - headPos.x, agentPos.y - headPos.y, agentPos.z - headPos.z);
            hm_Direction = dirOX + dirOY;

            hm_Ray = new Ray(headPos, hm_Direction);
            RaycastHit hm_RayHit;

            if (Physics.Raycast(hm_Ray, out hm_RayHit, rayLength, layerObstacles) && hm_RayHit.point != null)
            {
                ptsBuffer.Add(new Point3D((Int3)hm_RayHit.point, hm_RayHit.collider.name));
            }

            // Head-to-Ground.
            dirOY = new Vector3(groundPos.x - headPos.x, groundPos.y - headPos.y, groundPos.z - headPos.z);
            hg_Direction = dirOX + dirOY;

            hg_Ray = new Ray(headPos, hg_Direction);
            RaycastHit hg_RayHit;

            if (Physics.Raycast(hg_Ray, out hg_RayHit, rayLength, layerObstacles) && hg_RayHit.point != null)
            {
                ptsBuffer.Add(new Point3D((Int3)hg_RayHit.point, hg_RayHit.collider.name));
            }

            //dbg_rays.Add(new RayDebug(Color.blue, headPos, hg_Direction));
            //dbg_rays.Add(new RayDebug(Color.blue, headPos, hm_Direction));
            //dbg_rays.Add(new RayDebug(Color.blue, headPos, hh_Direction));

            observedPoints[angle].AddRange(ptsBuffer);
            ptsBuffer.Clear();
        }

        /// <summary>
        /// Запуск лучей из нижнего детектора
        /// </summary>
        /// <param name="angle">Угол поворота относительно вертикали</param>
        private void CastDownRays(int angle)
        {
            // Инициализация.
            Vector3 dirOX = groundScanner.right * rayLength;
            Vector3 dirOY;
            Vector3 agentPos = transform.position,
                    headPos = headScanner.position,
                    groundPos = groundScanner.position;

            if (!observedPoints.ContainsKey(angle))
            {
                observedPoints.Add(angle, new List<Point3D>());
            }

            // Ground-to-Ground
            gg_Direction = dirOX;

            gg_Ray = new Ray(groundPos, gg_Direction);
            RaycastHit gg_RayHit;

            if (Physics.Raycast(gg_Ray, out gg_RayHit, rayLength, layerObstacles) && gg_RayHit.point != null)
            {
                ptsBuffer.Add(new Point3D((Int3)gg_RayHit.point, gg_RayHit.collider.name));
            }
            else
            {
                ptsBuffer.Add((Point3D)(gg_Direction + groundPos));
            }

            // Ground-to-Middle.
            dirOY = new Vector3(agentPos.x - groundPos.x, agentPos.y - groundPos.y, agentPos.z - groundPos.z);
            gm_Direction = dirOX + dirOY;

            RaycastHit gm_RayHit;
            gm_Ray = new Ray(groundPos, gm_Direction);

            if (Physics.Raycast(gm_Ray, out gm_RayHit, rayLength, layerObstacles) && gm_RayHit.point != null)
            {
                ptsBuffer.Add(new Point3D((Int3)gm_RayHit.point, gm_RayHit.collider.name));
            }

            //Ground-to-Head
            dirOY = new Vector3(headPos.x - groundPos.x, headPos.y - groundPos.y, headPos.z - groundPos.z);
            gh_Direction = dirOY + dirOX;

            RaycastHit gh_RayHit;
            gh_Ray = new Ray(groundPos, gh_Direction);

            if (Physics.Raycast(gh_Ray, out gh_RayHit, rayLength, layerObstacles) && gh_RayHit.point != null)
            {
                ptsBuffer.Add(new Point3D((Int3)gh_RayHit.point, gh_RayHit.collider.name));
            }

            //dbg_rays.Add(new RayDebug(Color.red, groundPos, gg_Direction));
            //dbg_rays.Add(new RayDebug(Color.red, groundPos, gm_Direction));
            //dbg_rays.Add(new RayDebug(Color.red, groundPos, gh_Direction));
            observedPoints[angle].AddRange(ptsBuffer);
            ptsBuffer.Clear();
        }

        /// <summary>
        /// Поворот сканеров вокруг вертикальной оси.
        /// </summary>
        /// <param name="direction">Направление поворота ( = 1, по часовой стрелке; = -1, против часовой)</param>
        private void RotateRayScanners(int direction = 1)
        {
            RotateRayScanner(groundScanner, direction);
            RotateRayScanner(headScanner, direction);
        }

        /// <summary>
        /// Сбрасывает угол поворота всех сканеров.
        /// </summary>
        private void ResetAllScanners()
        {
            ResetScannerRotation(headScanner);
            ResetScannerRotation(groundScanner);
        }

        /// <summary>
        /// Поворот сканера на определенный угол.
        /// </summary>
        /// <param name="scanner">Сканер</param>
        private void RotateRayScanner(Transform scanner, int direction = 1)
        {
            scanner.Rotate(new Vector3(0f, turnOYAngle * direction, 0f));
        }

        /// <summary>
        /// Сбрасывает угол поворота сканера.
        /// </summary>
        /// <param name="scanner"></param>
        private void ResetScannerRotation(Transform scanner)
        {
            scanner.rotation = Quaternion.identity;
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
                    new Point3D(new Int3(points[index].position.x, ((Int3)groundScanner.position).y, points[index].position.z), points[index].obstacleName, points[index].type)
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
                if (vector1.GetAngleBetweenVectors(vector2) < maxSlopeAngle)
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
                        // 0 точек в наборе
                        case 0:
                            break;
                        // 1 точка в наборе. Делаем эту точку ключевой
                        case 1:
                            ptsSet[0] = HandleSinglePoint(ptsSet[0]);
                            observedPoints[key] = ptsSet;
                            break;
                        // 2 и более точек. Выделение избыточных точек. 
                        default:
                            ptsSet = DeleteDuplicatePoints(ptsSet);
                            ptsSet = DeleteOnlineExtraPoints(ptsSet);
                            ptsSet = DeleteExtraObjectPoints(ptsSet);

                            // Если после первых трех стадий обработки в словаре остается 1-а точка
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
                int key1, key2, key3;

                for (int i = 1; i < keys.Count - 1; i++)
                {
                    key1 = keys[i - 1];
                    key2 = keys[i];
                    key3 = keys[i + 1];

                    // TODO: дописать условие на принадлежность точки объекту.
                    if (observedPoints[key1].Count == 1 &&
                        observedPoints[key2].Count == 1 && observedPoints[key2][0].type != Point3DType.keyPt &&
                        observedPoints[key3].Count == 1 &&
                        GeneralGeometry.CheckByVectorsIfPointBelongsTo3DLine((Int3)observedPoints[key1][0], (Int3)observedPoints[key2][0], (Int3)observedPoints[key3][0], error)
                        )
                    {
                        observedPoints[keys[i]][0].type = Point3DType.extraPt;
                    }
                }

                observedPoints = observedPoints.Where(i => !(i.Value.Count == 1 && i.Value[0].type == Point3DType.extraPt)).ToDictionary(i => i.Key, i => i.Value);
            }
        }
        #endregion
    }
}