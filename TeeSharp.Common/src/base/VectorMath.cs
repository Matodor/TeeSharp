namespace TeeSharp.Common
{
    public static class VectorMath
    {
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
    }
}