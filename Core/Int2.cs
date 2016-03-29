using UnityEngine;
using System.Collections;
using System;

namespace MasterProject.Core
{
    public class Int2
    {
        public int x;
        public int z;

        public const int Precision = 1000;
        public const float FloatPrecision = 1000f;
        public const float PrecisionFactor = 0.001f;

        public double Magnitude
        {
            get
            {
                return Math.Sqrt(x * x + z * z);
            }
        }

        public Int2(int x, int z)
        {
            this.x = x;
            this.z = z;
        } 

        public Int2(Int3 ip)
        {
            x = ip.x;
            z = ip.z;
        }

        public static explicit operator Int2(Int3 ip)
        {
            return new Int2(ip.x, ip.z);
        }

        public static Int2 operator - (Int2 ip1, Int2 ip2)
        {
            return new Int2(ip1.x - ip2.x, ip1.z - ip2.z);
        }
    }
}
