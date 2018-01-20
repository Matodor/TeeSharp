using System.Diagnostics;

namespace TeeSharp.Core
{
    public static class Time
    {
        public static long Freq()
        {
            return Stopwatch.Frequency;
        }

        public static long Get()
        {
            return Stopwatch.GetTimestamp();
        }
    }
}