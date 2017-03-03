using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Teecsharp
{
    public struct vector2_float
    {
        public float x;
        public float y;
        
        public vector2_float(float nx, float ny)
        {
            x = nx;
            y = ny;
        }

        public override string ToString()
        {
            return x + " " + y;
        }

        public static vector2_float operator -(vector2_float v)
        {
            return new vector2_float(-v.x, -v.y);
        }

        public static vector2_float operator -(vector2_float l, vector2_float r)
        {
            return new vector2_float(l.x - r.x, l.y - r.y);
        }

        public static vector2_float operator +(vector2_float l, vector2_float r)
        {
            return new vector2_float(l.x + r.x, l.y + r.y);
        }

        public static vector2_float operator *(float v, vector2_float r)
        {
            return new vector2_float(r.x * v, r.y * v);
        }

        public static vector2_float operator *(vector2_float l, float v)
        {
            return new vector2_float(l.x * v, l.y * v);
        }

        public static vector2_float operator *(vector2_float l, vector2_float r)
        {
            return new vector2_float(l.x * r.x, l.y * r.y);
        }

        public static vector2_float operator /(float v, vector2_float r)
        {
            return new vector2_float(r.x / v, r.y / v);
        }

        public static vector2_float operator /(vector2_float l, float v)
        {
            return new vector2_float(l.x / v, l.y / v);
        }

        public static vector2_float operator /(vector2_float l, vector2_float r)
        {
            return new vector2_float(l.x / r.x, l.y / r.y);
        }

        public static bool operator <(vector2_float l, vector2_float r)
        {
            return l.x < r.x && l.x < r.x;
        }

        public static bool operator >(vector2_float l, vector2_float r)
        {
            return l.x > r.x && l.x > r.x;
        }

        public static bool operator <=(vector2_float l, vector2_float r)
        {
            return l.x <= r.x && l.x <= r.x;
        }
        
        public static bool operator >=(vector2_float l, vector2_float r)
        {
            return l.x >= r.x && l.x >= r.x;
        }
    }

    public static class VMath
    {
        public static float angle(vector2_float vec1, vector2_float vec2)
        {
            var diff = vec2 - vec1;
            var Angle = (float)(-1 * (Math.Atan2(diff.x, diff.y) * CMath.Rad2Deg) + 90);
            return (Angle < 0) ? 360 + Angle : Angle;
        }

        public static float length(vector2_float a)
        {
            return ((float)Math.Sqrt(a.x * a.x + a.y * a.y));
        }

        public static float distance(vector2_float a, vector2_float b)
        {
            return length(a - b);
        }

        public static float dot(vector2_float a, vector2_float b)
        {
            return a.x * b.x + a.y * b.y;
        }

        public static vector2_float normalize(vector2_float v)
        {
            float l = (float) (1.0f / Math.Sqrt(v.x * v.x + v.y * v.y));
            return new vector2_float(v.x * l, v.y * l);
        }

        public static vector2_float mix(vector2_float a, vector2_float b, float amount)
        {
            return a + (b - a) * amount;
        }

        private static float clamps(float val, float min, float max)
        {
            if (val < min)
                return min;
            if (val > max)
                return max;
            return val;
        }

        public static vector2_float closest_point_on_line(vector2_float line_point0, vector2_float line_point1, vector2_float target_point)
        {
            vector2_float c = target_point - line_point0;
            vector2_float v = line_point1 - line_point0;

            v = normalize(v);
            float d = length(line_point0 - line_point1);
            float t = dot(v, c) / d;

            return mix(line_point0, line_point1, clamps(t, 0, 1));
        }
    }
}
