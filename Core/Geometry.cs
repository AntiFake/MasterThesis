using UnityEngine;
using System.Collections;

namespace MasterProject.Core
{
    public static class Geometry
    {
        /// <summary>
        /// Проверка принадлежности точки прямой в пространстве.
        /// </summary>
        /// <param name="pt1">Точка 1</param>
        /// <param name="pt2">Точка 2 (проверяемая)</param>
        /// <param name="pt3">Точка 3</param>
        /// <param name="error">
        /// Погрешность (в процентах).
        /// Учитывая неточность вычислений вводится погрешность в n%
        /// Проверка для y осуществляется след. образом:
        /// y - 1% < x (z)
        /// y + 1% > x (z)
        /// При соблюдении этих двух условий считается, что точка принадлежит прямой.
        /// </param>
        /// <returns>true - точка принадлежит прямой, иначе - false.</returns>
        //public static bool CheckIfPointBelongsTo3DLine(Int3 pt1, Int3 pt2, Int3 pt3, float error) 
        //{
        //    // Ур-е:
        //    // x-x0   y-y0   z-z0
        //    // ---- = ---- = ----
        //    //  p1     p2     p3
        //    // Точка 1 (x0, y0, z0) - origin
        //    // Напр. вектор (p1, p2, p3) - direction

        //    // Получаем координаты вектора-направляющей для прямой.
        //    var direction = pt3 - pt1;
        //    int x = 0, y = 0, z = 0;

        //    // Считаем три отношения.
        //    if (direction.x != 0f)
        //    {
        //        x = (int)(((pt2.x - pt1.x) / (float)direction.x) * 1000);
        //    }
        //    if (direction.y != 0f)
        //    {
        //        y = (int)(((pt2.y - pt1.y) / (float)direction.y) * 1000);
        //    }
        //    if (direction.z != 0f)
        //    {
        //        z = (int)(((pt2.z - pt1.z) / (float)direction.z) * 1000);
        //    }

        //    // =========================================
        //    // Проверям принадлежность точки прямой
        //    // 1. Если ни одна из координат не равна 0, то проверяем x == y == z.
        //    // 2. Если 1 координата равна 0, то сравниваем две другие между собой.
        //    // 3. Если 2 координаты направляющего вектора равны 0, то точка однозначно принадлежит прямой.
        //    if (x == 0)
        //    {
        //        if (y == 0)
        //        {
        //            return true;
        //            //points[i].type = Point3DType.extraPt;
        //        }
        //        else
        //        {
        //            if (z == 0)
        //            {
        //                return true;
        //            }
        //            // if (y ≈≈ z)
        //            if ((y + y * error) >= z && (y - y * error) <= z)
        //            {
        //                return true;
        //            }
        //        }
        //    }
        //    else
        //    {
        //        if (y == 0)
        //        {
        //            if (z == 0)
        //            {
        //                return true;
        //            }
        //            // if (x ≈≈ z)
        //            if ((x + x * error) >= z && (x - x * error) <= z)
        //            {
        //                return true;
        //            }
        //        }
        //        else
        //        {
        //            if (z == 0)
        //            {
        //                // if (x ≈≈ y)
        //                if ((x + x * error) >= y && (x - x * error) <= y)
        //                {
        //                    return true;
        //                }
        //            }
        //            // if (x ≈≈ y) && (y ≈≈ z)
        //            if ((x + x * error) >= y && (x - x * error) <= y &&
        //                (y + y * error) >= z && (y - y * error) <= z)
        //            {
        //                return true;
        //            }
        //        }
        //    }

        //    return false;
        //}

        /// <summary>
        /// Определяет нахождение трех точек на одной прямой.
        /// Если два вектора коллинеарны (pt2 - pt1) и (pt3 - pt1), то считается, что точки лежат на одной прямой
        /// </summary>
        /// <param name="pt1">Тчк 1</param>
        /// <param name="pt2">Тчк 2</param>
        /// <param name="pt3">Тчк 3</param>
        /// <param name="error">Отклонение в градусах</param>
        /// <returns></returns>
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
}
