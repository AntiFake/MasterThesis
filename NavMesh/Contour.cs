using UnityEngine;
using System.Collections.Generic;
using MasterProject.Core;
using System.Linq;

namespace MasterProject.NavMesh
{
    /// <summary>
    /// Класс, описывающий точку контура.
    /// </summary>
    public class ContourPoint
    {
        public int measurementAngle;
        public Point3D point;
        public ContourPoint nextPoint;
        public ContourPoint prevPoint;

        public ContourPoint(int ma, Point3D pt)
        {
            measurementAngle = ma;
            point = pt;
        }
    }

    /// <summary>
    /// Класс, описывающий двусвязный кольцевой список точек контура.
    /// </summary>
    public class Contour
    {
        public ContourPoint currentPoint;

        /// <summary>
        /// Количество точек контура.
        /// </summary>
        public int Count
        {
            get
            {
                if (currentPoint == null)
                    return 0;

                int index = currentPoint.measurementAngle, count = 0;

                do
                {
                    currentPoint = currentPoint.nextPoint;
                    count++;
                }
                while (index != currentPoint.measurementAngle);

                return count;
            }
        }

        public Contour(Dictionary<int, List<Point3D>> pts)
        {
            int[] keys = pts.Keys.ToArray();
            ContourPoint start = null;

            for (int i = 1; i < keys.Length; i++)
            {
                if (currentPoint == null)
                    currentPoint = new ContourPoint(keys[i - 1], pts[keys[i - 1]][0]);

                currentPoint.nextPoint = new ContourPoint(keys[i], pts[keys[i]][0]);
                currentPoint.nextPoint.prevPoint = currentPoint;
                currentPoint = currentPoint.nextPoint;

                if (i == 1)
                    start = currentPoint.prevPoint;

                if (i == keys.Length - 1)
                {
                    currentPoint.nextPoint = start;
                    start.prevPoint = currentPoint;

                    //Возврат в начало.
                    currentPoint = currentPoint.nextPoint;
                }
            }
        }

        /// <summary>
        /// Перемещение в начало списка.
        /// </summary>
        public void MoveToStart()
        {
            if (currentPoint.measurementAngle > 180)
            {
                while (currentPoint.measurementAngle != 0)
                {
                    currentPoint = currentPoint.nextPoint;
                }
            }
            else
            {
                while (currentPoint.measurementAngle != 0)
                {
                    currentPoint = currentPoint.prevPoint;
                }
            }
        }

        /// <summary>
        /// Перемещение вперед на n элементов.
        /// </summary>
        /// <param name="n"></param>
        public void MoveForward(int n)
        {
            int i = 0;
            while (i < n)
            {
                currentPoint = currentPoint.nextPoint;
                i++;
            }
        }

        /// <summary>
        /// Перемещение назад на n элементов.
        /// </summary>
        /// <param name="n"></param>
        public void MoveBackward(int n)
        {
            int i = 0;
            while (i < n)
            {
                currentPoint = currentPoint.prevPoint;
                i++;
            }
        }

        /// <summary>
        /// Удаление текущей точки контура с перемещением.
        /// </summary>
        /// <param name="shiftForward">Перемещение вперед</param>
        public void DeleteCurrent(bool? shiftForward)
        {
            currentPoint.prevPoint.nextPoint = currentPoint.nextPoint;
            currentPoint.nextPoint.prevPoint = currentPoint.prevPoint;

            if (shiftForward.HasValue)
            {
                // Переход вперед.
                if (shiftForward == true)
                    currentPoint = currentPoint.nextPoint;
                // Переход назад.
                else
                    currentPoint = currentPoint.prevPoint;
            }
            else
                currentPoint = null;
        }
    }
}