namespace TeeSharp.Common
{
    public struct Vector2
    {
        public float Length => (float) System.Math.Sqrt(X * X + Y * Y);

        public Vector2 Normalized
        {
            get
            {
                var l = 1f / Length;
                return new Vector2(X * l, Y * l);
            }
        }

        public float X;
        public float Y;

        public Vector2(float x, float y)
        {
            X = x;
            Y = y;
        }

        public override string ToString()
        {
            return X + " " + Y;
        }

        public static Vector2 operator -(Vector2 v)
        {
            return new Vector2(-v.X, -v.Y);
        }

        public static Vector2 operator -(Vector2 l, Vector2 r)
        {
            return new Vector2(l.X - r.X, l.Y - r.Y);
        }

        public static Vector2 operator +(Vector2 l, Vector2 r)
        {
            return new Vector2(l.X + r.X, l.Y + r.Y);
        }

        public static Vector2 operator *(float v, Vector2 r)
        {
            return new Vector2(r.X * v, r.Y * v);
        }

        public static Vector2 operator *(Vector2 l, float v)
        {
            return new Vector2(l.X * v, l.Y * v);
        }

        public static Vector2 operator *(Vector2 l, Vector2 r)
        {
            return new Vector2(l.X * r.X, l.Y * r.Y);
        }

        public static Vector2 operator /(float v, Vector2 r)
        {
            return new Vector2(r.X / v, r.Y / v);
        }

        public static Vector2 operator /(Vector2 l, float v)
        {
            return new Vector2(l.X / v, l.Y / v);
        }

        public static Vector2 operator /(Vector2 l, Vector2 r)
        {
            return new Vector2(l.X / r.X, l.Y / r.Y);
        }
    }
}