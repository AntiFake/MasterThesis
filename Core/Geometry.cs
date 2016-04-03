using System;
using UnityEngine;

namespace MasterProject.Core
{
    public static class GeneralGeometry
    {
        public static bool CheckByVectorsIfPointBelongsTo3DLine(Int3 pt1, Int3 pt2, Int3 pt3, float error)
        {
            Int3 v1, v2;
            v1 = pt2 - pt1;
            v2 = pt3 - pt1;

            double angle = v1.GetAngleBetweenVectors(v2);

            if (angle >= (-1) * error && angle <= error)
                return true;

            return false;
        }
    }

    public static class CircleGeometry
    {
        /// <summary>
        /// Расчет радиуса вписанной окружности.
        /// </summary>
        /// <param name="pt1">Точка треугольника 1</param>
        /// <param name="pt2">Точка треугольника 2</param>
        /// <param name="pt3">Точка треугольника 3</param>
        /// <returns></returns>
        public static double GetInnerCircleRadius(Int3 pt1, Int3 pt2, Int3 pt3)
        {
            double a = (pt1 - pt2).Magnitude;
            double b = (pt1 - pt3).Magnitude;
            double c = (pt2 - pt3).Magnitude;

            double x = -a + b + c;
            double y = a - b + c;
            double z = a + b - c;
            double w = a + b + c;

            return Math.Sqrt((x * y * z) / (4 * w));
        }

        /// <summary>
        /// Расчет радиуса описанной окружности.
        /// </summary>
        /// <param name="pt1">Точка треугольника 1</param>
        /// <param name="pt2">Точка треугольника 2</param>
        /// <param name="pt3">Точка треугольника 3</param>
        /// <returns></returns>
        public static double GetOuterCircleRadius(Int3 pt1, Int3 pt2, Int3 pt3)
        {
            double a = (pt1 - pt2).Magnitude;
            double b = (pt1 - pt3).Magnitude;
            double c = (pt2 - pt3).Magnitude;
            double p = 0.5 * (a + b + c);

            double r = (a * b * c) / (4 * Math.Sqrt(p * (p - a) * (p - b) * (p - c)));

            return r;
        }
    }
}
