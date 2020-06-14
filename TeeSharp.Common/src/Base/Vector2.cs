namespace TeeSharp.Common
{
    public struct Vector2
    {
        public static Vector2 Zero = new Vector2(0, 0);
        public static Vector2 One = new Vector2(1, 1);

        public float Length => (float) System.Math.Sqrt(x * x + y * y);

        public Vector2 Normalized
        {
            get
            {
                var l = 1f / Length;
                return new Vector2(x * l, y * l);
            }
        }

        public float x;
        public float y;

        public Vector2(float x, float y)
        {
            this.x = x;
            this.y = y;
        }

        public override string ToString()
        {
            return x + " " + y;
        }

        public static Vector2 operator -(Vector2 v)
        {
            return new Vector2(-v.x, -v.y);
        }

        public static Vector2 operator -(Vector2 l, Vector2 r)
        {
            return new Vector2(l.x - r.x, l.y - r.y);
        }

        public static Vector2 operator +(Vector2 l, Vector2 r)
        {
            return new Vector2(l.x + r.x, l.y + r.y);
        }

        public static Vector2 operator *(float v, Vector2 r)
        {
            return new Vector2(r.x * v, r.y * v);
        }

        public static Vector2 operator *(Vector2 l, float v)
        {
            return new Vector2(l.x * v, l.y * v);
        }

        public static Vector2 operator *(Vector2 l, Vector2 r)
        {
            return new Vector2(l.x * r.x, l.y * r.y);
        }

        public static Vector2 operator /(float v, Vector2 r)
        {
            return new Vector2(r.x / v, r.y / v);
        }

        public static Vector2 operator /(Vector2 l, float v)
        {
            return new Vector2(l.x / v, l.y / v);
        }

        public static Vector2 operator /(Vector2 l, Vector2 r)
        {
            return new Vector2(l.x / r.x, l.y / r.y);
        }
    }
}