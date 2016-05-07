using MasterProject.Core;
using System;
using System.Collections.Generic;

namespace MasterProject.NavMesh
{
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

        /// <summary>
        /// Построение контура.
        /// </summary>
        /// <param name="pts">Набор точек для построения контура</param>
        public Contour(List<Point3D> pts)
        {
            ContourPoint start = null;

            for (int i = 1; i < pts.Count; i++)
            {
                if (currentPoint == null)
                    currentPoint = new ContourPoint(Guid.NewGuid(), pts[i - 1]);

                currentPoint.nextPoint = new ContourPoint(Guid.NewGuid(), pts[i]);
                currentPoint.nextPoint.prevPoint = currentPoint;
                currentPoint = currentPoint.nextPoint;

                if (i == 1)
                    start = currentPoint.prevPoint;

                if (i == pts.Count - 1)
                {
                    currentPoint.nextPoint = start;
                    start.prevPoint = currentPoint;

                    //Возврат в начало.
                    currentPoint = currentPoint.nextPoint;
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