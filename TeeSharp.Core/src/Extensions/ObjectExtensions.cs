using System;
using System.Runtime.CompilerServices;

namespace TeeSharp.Core.Extensions;

public static class ObjectExtensions
{
    public static T Tap<T>(this T target, Action<T> callback)
    {
        callback(target);
        return target;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Cast<T>(this object target)
    {
        return (T) target;
    }
}
