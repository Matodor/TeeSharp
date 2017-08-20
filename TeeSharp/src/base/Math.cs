using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeeSharp
{
    public static class Math
    {
        public const float Deg2Rad = 0.01745329F;
        public const float Rad2Deg = 57.29578F;

        public static int Clamp(int val, int min, int max)
        {
            if (val < min)
                return min;
            if (val > max)
                return max;
            return val;
        }

        public static double Clamp(double val, double min, double max)
        {
            if (val < min)
                return min;
            if (val > max)
                return max;
            return val;
        }

        public static float Clamp(float val, float min, float max)
        {
            if (val < min)
                return min;
            if (val > max)
                return max;
            return val;
        }

        public static float Sign(float f)
        {
            return f < 0.0f ? -1.0f : 1.0f;
        }

        public static int RoundToInt(float f)
        {
            if (f > 0)
                return (int)(f + 0.5f);
            return (int)(f - 0.5f);
        }

        public static int Mix(int a, int b, int amount)
        {
            return a + (b - a) * amount;
        }

        public static float Mix(float a, float b, float amount)
        {
            return a + (b - a) * amount;
        }

        public static double Mix(double a, double b, double amount)
        {
            return a + (b - a) * amount;
        }
    }
}
