using System;

namespace TeeSharp.Core.Helpers
{
    public static class SecurityHelper
    {
        public static ulong KnuthHash(Span<byte> buffer)
        {
            var hashedValue = 3074457345618258791ul;
            for (var i = 0; i < buffer.Length; i++)
            {
                hashedValue += buffer[i];
                hashedValue *= 3074457345618258799ul;
            }
            return hashedValue;
        }
        
        public static ulong KnuthHash(ulong hashedValue, Span<byte> buffer)
        {
            for (var i = 0; i < buffer.Length; i++)
            {
                hashedValue += buffer[i];
                hashedValue *= 3074457345618258799ul;
            }
            return hashedValue;
        }
    }
}