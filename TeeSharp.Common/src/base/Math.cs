namespace TeeSharp.Common
{
    public static class Math
    {
        public const float DEG2RAD = 0.01745329F;
        public const float RAD2DEG = 57.29578F;

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

        public static int SaturatedAdd(int min, int max,
            int current, int modifier)
        {
            if (modifier < 0)
            {
                if (current < min)
                    return current;
                current += modifier;
                if (current < min)
                    current = min;
                return current;
            }

            if (current > max)
                return current;
            current += modifier;
            if (current > max)
                current = max;
            return current;
        }

        public static float SaturatedAdd(float min, float max, 
            float current, float modifier)
        {
            if (modifier < 0)
            {
                if (current < min)
                    return current;
                current += modifier;
                if (current < min)
                    current = min;
                return current;
            }

            if (current > max)
                    return current;
                current += modifier;
                if (current > max)
                    current = max;
                return current;
        }

        public static double SaturatedAdd(double min, double max,
            double current, double modifier)
        {
            if (modifier < 0)
            {
                if (current < min)
                    return current;
                current += modifier;
                if (current < min)
                    current = min;
                return current;
            }

            if (current > max)
                return current;
            current += modifier;
            if (current > max)
                current = max;
            return current;
        }

        public static float Distance(Vec2 a, Vec2 b)
        {
            return (a - b).Length;
        }

        public static float Dot(Vec2 a, Vec2 b)
        {
            return a.x * b.x + a.y * b.y;
        }

        public static Vec2 Mix(Vec2 a, Vec2 b, float amount)
        {
            return a + (b - a) * amount;
        }

        public static Vec2 CalcPos(Vec2 pos, Vec2 velocity, float curvature, 
            float speed, float time)
        {
            Vec2 n;
            time *= speed;
            n.x = pos.x + velocity.x * time;
            n.y = pos.y + velocity.y * time + curvature / 10000 * (time * time);
            return n;
        }

        public static Vec2 ClosestPointOnLine(Vec2 linePoint0, Vec2 linePoint1,
            Vec2 targetPoint)
        {
            var c = targetPoint - linePoint0;
            var v = (linePoint1 - linePoint0).Normalized;
            var l = (linePoint0 - linePoint1).Length;
            var t = Dot(v, c) / l;
            return Mix(linePoint0, linePoint1, System.Math.Clamp(t, 0, 1));
        }

        public static float VelocityRamp(float value, float start,
            float range, float curvature)
        {
            if (value < start)
                return 1.0f;
            return (float)(1.0f / System.Math.Pow(curvature, (value - start) / range));
        }

        public static float GetAngle(Vec2 Dir)
        {
            if (Dir.x == 0 && Dir.y == 0)
                return 0.0f;
            float a = (float)System.Math.Atan(Dir.y / Dir.x);
            if (Dir.x < 0)
                a = (float) (a + System.Math.PI);
            return a;
        }
    }
}
