namespace TeeSharp.Common
{
    public static class Math
    {
        public const float Deg2Rad = 0.01745329F;
        public const float Rad2Deg = 57.29578F;

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
