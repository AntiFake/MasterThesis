using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using MasterProject.Core;
using MasterProject.VisualDebug;
using MasterProject.NavMesh;
using MasterProject.Agent.Scanner;
using MasterProject.Agent.Approximation;
using MasterProject.Agent.ContourBuilder;

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
        private MeasurementApproximation measurementApproximation;
        private GeneralApproximation generalApproximation;
        private OutlineBuilder outlineBuilder;

        // Словарь для хранения найденных точек.
        // Градус отклонения лучей - ключ, массив точек - список значений.
        [HideInInspector]
        public Dictionary<int, List<Point3D>> observedPoints;

        [HideInInspector]
        public Int3 headScannerInt3Pos;

        [HideInInspector]
        public Int3 groundScannerInt3Pos;

        // Механизм разбиения проходимой области на треугольники.
        private List<Contour> contours;
        private Triangulator triangulator;
        private List<Triangle> passableArea;
        private NavMeshGraph navMesh;

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
            measurementApproximation = new MeasurementApproximation();
            generalApproximation = new GeneralApproximation();
            outlineBuilder = new OutlineBuilder();
        }

        public void Start()
        {
            ScanArea();
        }
        #endregion

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
    }
}