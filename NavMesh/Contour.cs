using UnityEngine;
using System.Collections.Generic;
using MasterProject.Core;
using System.Linq;
using System;

namespace MasterProject.NavMesh
{
    /// <summary>
    /// Класс, описывающий точку контура.
    /// </summary>
    public class ContourPoint
    {
        public Guid uniqueIndex;
        public Point3D point;
        public ContourPoint nextPoint;
        public ContourPoint prevPoint;

        public ContourPoint(Guid ma, Point3D pt)
        {
            uniqueIndex = ma;
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

                Guid index = currentPoint.uniqueIndex;
                int count = 0;

                do
                {
                    currentPoint = currentPoint.nextPoint;
                    count++;
                }
                while (index != currentPoint.uniqueIndex);

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
                    currentPoint = new ContourPoint(Guid.NewGuid(), pts[keys[i - 1]][0]);

                currentPoint.nextPoint = new ContourPoint(Guid.NewGuid(), pts[keys[i]][0]);
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

        public Contour(Dictionary<int, List<Point3D>> slope, string str)
        {
            List<KeyValuePair<int, Point3D>> slopeBoundaries = GetSlopeBoundaries(slope);
            ContourPoint start = null;

            for (int i = 1; i < slopeBoundaries.Count; i++)
            {
                if (currentPoint == null)
                    currentPoint = new ContourPoint(Guid.NewGuid(), slopeBoundaries[i - 1].Value);

                currentPoint.nextPoint = new ContourPoint(Guid.NewGuid(), slopeBoundaries[i].Value);
                currentPoint.nextPoint.prevPoint = currentPoint;
                currentPoint = currentPoint.nextPoint;

                if (i == 1)
                    start = currentPoint.prevPoint;

                if (i == slopeBoundaries.Count - 1)
                {
                    currentPoint.nextPoint = start;
                    start.prevPoint = currentPoint;
                    currentPoint = currentPoint.nextPoint;
                }
            }
        }

        /// <summary>
        /// Построение границ наклонной плоскости.
        /// </summary>
        /// <param name="slope">Набор точек наклонной плоскости</param>
        /// <returns></returns>
        private List<KeyValuePair<int, Point3D>> GetSlopeBoundaries(Dictionary<int, List<Point3D>> slope)
        {
            List<KeyValuePair<int, Point3D>> left = new List<KeyValuePair<int, Point3D>>();
            List<KeyValuePair<int, Point3D>> top = new List<KeyValuePair<int, Point3D>>();
            List<KeyValuePair<int, Point3D>> right = new List<KeyValuePair<int, Point3D>>();
            List<KeyValuePair<int, Point3D>> bottom = new List<KeyValuePair<int, Point3D>>();
            List<KeyValuePair<int, Point3D>> slopeBoundaries = new List<KeyValuePair<int, Point3D>>();

            int[] keys = slope.Keys.ToArray();

            // Левая сторона.
            left = slope[keys[0]].Select(i => new KeyValuePair<int, Point3D>(keys[0], i)).ToList();
            // Правая сторона.
            right = slope[keys[keys.Length - 1]].Select(i => new KeyValuePair<int, Point3D>(keys[keys.Length - 1], i)).ToList();
            right.Reverse();
            // Верхняя сторона.
            for (int i = 1; i < slope.Count - 1; i++)
            {
                top.Add(new KeyValuePair<int, Point3D>(keys[i], slope[keys[i]][slope[keys[i]].Count - 1]));
            }
            // Нижняя сторона.
            for (int i = slope.Count - 1; i > 0; i--)
            {
                bottom.Add(new KeyValuePair<int, Point3D>(keys[i], slope[keys[i]][0]));
            }

            slopeBoundaries.AddRange(left);
            slopeBoundaries.AddRange(top);
            slopeBoundaries.AddRange(right);
            slopeBoundaries.AddRange(bottom);

            return slopeBoundaries;
        }

        /// <summary>
        /// Поиск всех наклонных плоскостей.
        /// </summary>
        /// <param name="pts">Точки, полученные в результате исследования.</param>
        /// <returns></returns>
        public static List<Dictionary<int, List<Point3D>>> Slopes(Dictionary<int, List<Point3D>> pts)
        {
            int[] keys = pts.Keys.ToArray();
            List<Dictionary<int, List<Point3D>>> slopes = new List<Dictionary<int, List<Point3D>>>();

            for (int i = 0; i < keys.Length; i++)
            {
                if (pts[keys[i]].Count > 1)
                {
                    int j = i;
                    Dictionary<int, List<Point3D>> slopeContour = new Dictionary<int, List<Point3D>>();

                    while (pts[keys[j]].Count > 1 && j < keys.Length)
                    {
                        slopeContour.Add(keys[j], pts[keys[j]]);
                        j++;
                    }

                    if (slopeContour.Count > 1)
                        slopes.Add(slopeContour);

                    i = j;
                }
            }

            return slopes;
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