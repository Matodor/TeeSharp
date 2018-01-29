namespace TeeSharp.Common
{
    public static class VectorMath
    {
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
    }
}