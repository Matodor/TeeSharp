namespace TeeSharp.Common
{
    public struct Vec2
    {
        public static Vec2 zero = new Vec2(0, 0);
        public static Vec2 one = new Vec2(1, 1);

        public float Length => (float) System.Math.Sqrt(x * x + y * y);

        public Vec2 Normalized
        {
            get
            {
                var l = 1f / Length;
                return new Vec2(x * l, y * l);
            }
        }

        public float x;
        public float y;

        public Vec2(float x, float y)
        {
            this.x = x;
            this.y = y;
        }

        public override string ToString()
        {
            return x + " " + y;
        }

        public static Vec2 operator -(Vec2 v)
        {
            return new Vec2(-v.x, -v.y);
        }

        public static Vec2 operator -(Vec2 l, Vec2 r)
        {
            return new Vec2(l.x - r.x, l.y - r.y);
        }

        public static Vec2 operator +(Vec2 l, Vec2 r)
        {
            return new Vec2(l.x + r.x, l.y + r.y);
        }

        public static Vec2 operator *(float v, Vec2 r)
        {
            return new Vec2(r.x * v, r.y * v);
        }

        public static Vec2 operator *(Vec2 l, float v)
        {
            return new Vec2(l.x * v, l.y * v);
        }

        public static Vec2 operator *(Vec2 l, Vec2 r)
        {
            return new Vec2(l.x * r.x, l.y * r.y);
        }

        public static Vec2 operator /(float v, Vec2 r)
        {
            return new Vec2(r.x / v, r.y / v);
        }

        public static Vec2 operator /(Vec2 l, float v)
        {
            return new Vec2(l.x / v, l.y / v);
        }

        public static Vec2 operator /(Vec2 l, Vec2 r)
        {
            return new Vec2(l.x / r.x, l.y / r.y);
        }
    }
}