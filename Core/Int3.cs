using UnityEngine;
using System;

namespace MasterProject.Core
{
    public class Int3
    {
        public int x;
        public int y;
        public int z;

        public const float FloatPrecision = 1000f;
        public const float PrecisionFactor = 0.001f;

        public double Magnitude
        {
            get
            {
                return Math.Sqrt((double)x * (double)x + (double)y * (double)y + (double)z * (double)z);
            }
        }

        public Vector3 Normal
        {
            get
            {
                return new Vector3 (
                    x  / (float) Magnitude,
                    y / (float) Magnitude,
                    z / (float) Magnitude
                );
            }
        }
        //public Int3 Right
        //{
        //    get
        //    {
        //        return (Int3) (Quaternion.Euler(0, 0, 90) * (Vector3)this);
        //    }
        //}

        public Int3(Vector3 position)
        {
            x = (int)Math.Round(position.x * FloatPrecision);
            y = (int)Math.Round(position.y * FloatPrecision);
            z = (int)Math.Round(position.z * FloatPrecision);
        }

        public Int3(Int3 ip)
        {
            x = ip.x;
            y = ip.y;
            z = ip.z;
        }

        public Int3(int x, int y, int z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

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

        public static explicit operator Int3(Point3D pt)
        {
            return new Int3(pt.position.x, pt.position.y, pt.position.z);
        }

        public static Int3 operator - (Int3 ip_1, Int3 ip_2)
        {
            return new Int3(ip_1.x - ip_2.x, ip_1.y - ip_2.y, ip_1.z - ip_2.z);
        }

        public static int operator * (Int3 ip_1, Int3 ip_2)
        {
            return ip_1.x * ip_2.x + ip_1.y * ip_2.y + ip_1.z * ip_2.z;
        }

        public static Int3 operator + (Int3 ip_1, Int3 ip_2)
        {
            return new Int3(ip_1.x + ip_2.x, ip_1.y + ip_2.y, ip_1.z + ip_2.z);
        }

        public static Int3 operator / (Int3 ip, int divider)
        {
            return new Int3(ip.x / divider, ip.y / divider, ip.z / divider);
        }

        /// <summary>
        /// Угол между векторами (в градусах)
        /// </summary>
        /// <param name="vect">Вектор</param>
        /// <returns>Угол между векторами</returns>
        public double GetAngle(Int3 vect)
        {
            ///           a * b
            /// arccos( --------- )
            ///         |a| * |b|
            int sm = this * vect;
            double mm = Magnitude * vect.Magnitude;
            return Math.Acos(sm / mm) * 180 / Math.PI;
        }
    }
}