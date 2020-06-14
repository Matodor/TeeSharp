using System;

namespace TeeSharp.Common
{
    public static class MathHelper
    {
        public const float Deg2Rad = 0.0174532925199433f;
        public const float Rad2Deg = 57.2957795130823f;

        public static int RoundToInt(float f)
        {
            if (f > 0)
                return (int) (f + 0.5f);
            return (int) (f - 0.5f);
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

        public static float Distance(Vector2 a, Vector2 b)
        {
            return (a - b).Length;
        }

        public static float Dot(Vector2 a, Vector2 b)
        {
            return a.x * b.x + a.y * b.y;
        }

        public static Vector2 Mix(Vector2 a, Vector2 b, float amount)
        {
            return a + (b - a) * amount;
        }

        public static Vector2 CalcPos(Vector2 pos, Vector2 velocity, float curvature,
            float speed, float time)
        {
            time *= speed;
            return new Vector2(
                pos.x + velocity.x * time,
                pos.y + velocity.y * time + curvature / 10000 * (time * time));
        }

        public static Vector2 ClosestPointOnLine(Vector2 linePoint0, Vector2 linePoint1,
            Vector2 targetPoint)
        {
            var c = targetPoint - linePoint0;
            var v = (linePoint1 - linePoint0).Normalized;
            var l = (linePoint0 - linePoint1).Length;
            var t = Dot(v, c) / l;
            return Mix(linePoint0, linePoint1, Math.Clamp(t, 0, 1));
        }

        public static float VelocityRamp(float value, float start,
            float range, float curvature)
        {
            if (value < start)
                return 1.0f;
            return (float) (1.0f / Math.Pow(curvature, (value - start) / range));
        }

        public static float Angle(Vector2 v)
        {
            return (float) Math.Atan2(v.y, v.x);
        }

        //public static float GetAngle(Vector2 Dir)
        //{
        //    if (Dir.x == 0 && Dir.y == 0)
        //        return 0.0f;
        //    float a = (float)System.Math.Atan(Dir.y / Dir.x);
        //    if (Dir.x < 0)
        //        a = (float) (a + System.Math.PI);
        //    return a;
        //}
    }
}
