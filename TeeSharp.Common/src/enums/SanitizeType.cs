using System;

namespace TeeSharp.Common.Enums
{
    [Flags]
    public enum SanitizeType
    {
        Sanitize = 1 << 0,
        SanitizeCC = 1 << 1,
        SkipStartWhitespaces = 1 << 2
    }
}