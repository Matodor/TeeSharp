namespace TeeSharp.Common
{
    public static class VectorMath
    {
        public static float Distance(vec2 a, vec2 b)
        {
            return (a - b).Length;
        }

        public static float Dot(vec2 a, vec2 b)
        {
            return a.x * b.x + a.y * b.y;
        }

        public static vec2 Mix(vec2 a, vec2 b, float amount)
        {
            return a + (b - a) * amount;
        }
    }
}