using System.Collections.Generic;
using UnityEngine;
using System;

namespace MastersLomasters
{
    public enum MeshType
    {
        Rectangle,
        Circle,
        CustomMesh
    }

    //Структура, описывающая вектор на плоскости
    public struct Int2
    {
        public int x;
        public int y;

        public Int2(int x, int y)
        {
            this.x = x;
            this.y = y;
        }

        /// <summary>
        /// Квадрат длины вектора 
        /// </summary>
        public int sqrMagnitude
        {
            get
            {
                return x * x + y * y;
            }
        }

        /// <summary>
        /// Квадрат длины вектора (long type)
        /// </summary>
        public long sqrMagnitudeLong
        {
            get
            {
                return (long)x * (long)x + (long)y * (long)y;
            }
        }

        /// <summary>
        /// Сложение векторов
        /// </summary>
        /// <param name="a">Вектор A</param>
        /// <param name="b">Вектор B</param>
        /// <returns></returns>
        public static Int2 operator +(Int2 a, Int2 b)
        {
            return new Int2(a.x + b.x, a.y + b.y);
        }

        /// <summary>
        /// Разность векторов
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static Int2 operator -(Int2 a, Int2 b)
        {
            return new Int2(a.x - b.x, a.y - b.y);
        }

        /// <summary>
        /// Равенство векторов
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static bool operator ==(Int2 a, Int2 b)
        {
            return a.x == b.x && a.y == b.y;
        }

        /// <summary>
        /// Неравенство векторов
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static bool operator !=(Int2 a, Int2 b)
        {
            return a.x != b.x || a.y != b.y;
        }

        /// <summary>
        /// Скалярное произведение векторов - операция над двумя векторами, 
        /// результатом которой является число [когда рассматриваются векторы, числа часто называют скалярами], 
        /// не зависящее от системы координат и характеризующее длины векторов-сомножителей и угол между ними
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static int Dot(Int2 a, Int2 b)
        {
            return a.x * b.x + a.y * b.y;
        }

        /// <summary>
        /// Скалярное произведение для long
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static long DotLong(Int2 a, Int2 b)
        {
            return (long)a.x * (long)b.x + (long)a.y * (long)b.y;
        }

        /// <summary>
        /// Равенство двух векторов
        /// </summary>
        /// <param name="o"></param>
        /// <returns></returns>
        public override bool Equals(System.Object o)
        {
            if (o == null) return false;
            Int2 rhs = (Int2)o;

            return x == rhs.x && y == rhs.y;
        }

        /// <summary>
        /// Получение какого-то хеша
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return x * 49157 + y * 98317;
        }

        /// <summary>
        /// Формирование нового вектора из минимальных координат двух других векторов
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static Int2 Min(Int2 a, Int2 b)
        {
            return new Int2(System.Math.Min(a.x, b.x), System.Math.Min(a.y, b.y));
        }


        /// <summary>
        /// Формирование нового вектора из максимальных координат двух других векторов
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static Int2 Max(Int2 a, Int2 b)
        {
            return new Int2(System.Math.Max(a.x, b.x), System.Math.Max(a.y, b.y));
        }

        //public static int2 fromint3xz(int3 o)
        //{
        //    return new int2(o.x, o.z);
        //}

        //public static int3 toint3xz(int2 o)
        //{
        //    return new int3(o.x, 0, o.y);
        //}

        public override string ToString()
        {
            return "(" + x + ", " + y + ")";
        }
    }

    /// <summary>
    /// Хранит координаты в типе integer
    /// </summary>
    public struct Int3
    {
        public int x;
        public int y;
        public int z;

        public const int Precision = 1000;
        public const float FloatPrecision = 1000f;
        public const float PrecisionFactor = 0.001f;

        public Int3(Vector3 position)
        {
            x = (int)Math.Round(position.x * FloatPrecision);
            y = (int)Math.Round(position.y * FloatPrecision);
            z = (int)Math.Round(position.z * FloatPrecision);
        }

        public Int3(int x, int y, int z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        /// <summary>
        /// Приведение типа из Vector3 в Int3
        /// </summary>
        /// <param name="v"></param>
        public static explicit operator Int3(Vector3 v)
        {
            return new Int3(
                (int)Math.Round(v.x * FloatPrecision),
                (int)Math.Round(v.y * FloatPrecision),
                (int)Math.Round(v.z * FloatPrecision)
            );
        }

        public static explicit operator Vector3(Int3 ip)
        {
            return new Vector3(
                ip.x * PrecisionFactor,
                ip.y * PrecisionFactor,
                ip.z * PrecisionFactor
                );
        }
    }

    public struct IntPoint
    {
        public long X;
        public long Y;

        public IntPoint(IntPoint pt)
        {
            this = pt;
        }

        public IntPoint(long X, long Y)
        {
            this.X = X;
            this.Y = Y;
        }
    }

    public static class ListPool<T>
    {
        static List<List<T>> pool;

        static ListPool() 
        {
            pool = new List<List<T>>();
        }

        /// <summary>
        /// Возвращает последний список из пула.
        /// Если пул пуст, то возвращает пустой список
        /// </summary>
        /// <returns></returns>
        public static List<T> Claim()
        {
            lock (pool)
            {
                if (pool.Count > 0)
                {
                    List<T> ls = pool[pool.Count - 1];
                    pool.RemoveAt(pool.Count - 1);
                    return ls;
                }
                else
                {
                    return new List<T>();
                }
            }
        }

        /// <summary>
        /// Переинециализация списка пула 
        /// </summary>
        /// <param name="list"></param>
        public static void Release(List<T> list)
        {
            list.Clear();
            lock(pool)
            {
                for (int i = 0; i < pool.Count; i++)
                    if (pool[i] == list)
                        throw new System.InvalidOperationException("Список уже должен быть пустым");
                pool.Add(list);
            }
        }
    }
}