using System;

namespace TeeSharp.Core
{ 
    [Flags]
    public enum SanitizeType
    {
        Sanitize = 1 << 0,
        SanitizeCC = 1 << 1,
        SkipStartWhitespaces = 1 << 2
    }
}