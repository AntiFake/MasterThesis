using System;
using UnityEngine;
using MasterProject.NavMesh;

namespace MasterProject.Core
{
    public static class GeneralGeometry
    {
        public static bool IsPointBelongsTo3DLine(Int3 pt_1, Int3 pt_2, Int3 pt_3, float error)
        {
            Int3 v_1, v_2;
            v_1 = pt_2 - pt_1;
            v_2 = pt_3 - pt_1;

            double angle = v_1.GetAngle(v_2);

            if (angle >= (-1) * error && angle <= error)
                return true;

            return false;
        }

        public static bool IsPointInsideTriangle(Triangle t, Vector3 checked_pt)
        {
            Vector3 v_1, v_2, v_3;
            float dot_11, dot_12, dot_13, dot_22, dot_23, invDenom, u, v;

            v_1 = (Vector3)(t.pt_2.position - t.pt_1.position);
            v_2 = (Vector3)(t.pt_3.position - t.pt_1.position);
            v_3 = checked_pt - (Vector3) t.pt_1.position;

            dot_11 = Vector3.Dot(v_1, v_1);
            dot_12 = Vector3.Dot(v_1, v_2);
            dot_13 = Vector3.Dot(v_1, v_3);
            dot_22 = Vector3.Dot(v_2, v_2);
            dot_23 = Vector3.Dot(v_2, v_3);

            // Расчет барицентрических координат. https://ru.wikipedia.org/wiki/%D0%91%D0%B0%D1%80%D0%B8%D1%86%D0%B5%D0%BD%D1%82%D1%80%D0%B8%D1%87%D0%B5%D1%81%D0%BA%D0%B8%D0%B5_%D0%BA%D0%BE%D0%BE%D1%80%D0%B4%D0%B8%D0%BD%D0%B0%D1%82%D1%8B
            invDenom = 1 / (dot_11 * dot_22 - dot_12 * dot_12);
            u = (dot_22 * dot_13 - dot_12 * dot_23) * invDenom;
            v = (dot_11 * dot_23 - dot_12 * dot_13) * invDenom;

            return (u >= 0) && (v >= 0) && (u + v < 1);
        }
    }

    public static class CircleGeometry
    {
        /// <summary>
        /// Расчет радиуса вписанной окружности.
        /// </summary>
        /// <param name="pt_1">Точка треугольника 1</param>
        /// <param name="pt_2">Точка треугольника 2</param>
        /// <param name="pt_3">Точка треугольника 3</param>
        /// <returns></returns>
        public static double GetInnerCircleRadius(Int3 pt_1, Int3 pt_2, Int3 pt_3)
        {
            double a = (pt_1 - pt_2).Magnitude;
            double b = (pt_1 - pt_3).Magnitude;
            double c = (pt_2 - pt_3).Magnitude;

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
