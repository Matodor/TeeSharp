using System;

namespace TeeSharp.Common
{
    public static class RNG
    {
        private static readonly Random _random = new Random();
        private static readonly object _sync = new object();

        public static int Int()
        {
            lock (_sync)
            {
                return _random.Next();
            }
        }
    }
}