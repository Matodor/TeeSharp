namespace TeeSharp.Common
{
    public struct vec2
    {
        public static vec2 zero = new vec2(0, 0);
        public static vec2 one = new vec2(1, 1);

        public float Length => (float) System.Math.Sqrt(x * x + y * y);

        public vec2 Normalized
        {
            get
            {
                var l = 1f / Length;
                return new vec2(x * l, y * l);
            }
        }

        public float x;
        public float y;

        public vec2(float x, float y)
        {
            this.x = x;
            this.y = y;
        }

        public override string ToString()
        {
            return x + " " + y;
        }

        public static vec2 operator -(vec2 v)
        {
            return new vec2(-v.x, -v.y);
        }

        public static vec2 operator -(vec2 l, vec2 r)
        {
            return new vec2(l.x - r.x, l.y - r.y);
        }

        public static vec2 operator +(vec2 l, vec2 r)
        {
            return new vec2(l.x + r.x, l.y + r.y);
        }

        public static vec2 operator *(float v, vec2 r)
        {
            return new vec2(r.x * v, r.y * v);
        }

        public static vec2 operator *(vec2 l, float v)
        {
            return new vec2(l.x * v, l.y * v);
        }

        public static vec2 operator *(vec2 l, vec2 r)
        {
            return new vec2(l.x * r.x, l.y * r.y);
        }

        public static vec2 operator /(float v, vec2 r)
        {
            return new vec2(r.x / v, r.y / v);
        }

        public static vec2 operator /(vec2 l, float v)
        {
            return new vec2(l.x / v, l.y / v);
        }

        public static vec2 operator /(vec2 l, vec2 r)
        {
            return new vec2(l.x / r.x, l.y / r.y);
        }
    }
}