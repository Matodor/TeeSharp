namespace Teecsharp
{
    public static class CMath
    {
        public const float Deg2Rad = 0.01745329F;
        public const float Rad2Deg = 57.29578F;
        public const float pi = 3.1415926535897932384626433f;

        public static int clamp(int val, int min, int max)
        {
            if (val < min)
                return min;
            if (val > max)
                return max;
            return val;
        }

        public static double clamp(double val, double min, double max)
        {
            if (val < min)
                return min;
            if (val > max)
                return max;
            return val;
        }

        public static float clamp(float val, float min, float max)
        {
            if (val < min)
                return min;
            if (val > max)
                return max;
            return val;
        }

        public static float sign(float f)
        {
            return f < 0.0f ? -1.0f : 1.0f;
        }

        public static int round_to_int(float f)
        {
            if (f > 0)
                return (int)(f + 0.5f);
            return (int)(f - 0.5f);
        }

        public static int mix(int a, int b, int amount)
        {
            return a + (b - a) * amount;
        }

        public static float mix(float a, float b, float amount)
        {
            return a + (b - a) * amount;
        }

        public static double mix(double a, double b, double amount)
        {
            return a + (b - a) * amount;
        }

        public static float frandom()
        {
            return 0;//new Random().NextDouble() / (float)(RAND_MAX);
        }

        // float to fixed
        public static int f2fx(float v)
        {
            return (int)(v * (float)(1 << 10));
        }

        public static float fx2f(int v)
        {
            return v * (1.0f / (1 << 10));
        }

        public static int gcd(int a, int b)
        {
            while (b != 0)
            {
                int c = a % b;
                a = b;
                b = c;
            }
            return a;
        }
    }
}
