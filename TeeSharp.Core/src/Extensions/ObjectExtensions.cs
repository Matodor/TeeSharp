using System;

namespace TeeSharp.Core.Extensions;

public static class ObjectExtensions
{
    public static T Tap<T>(this T target, Action<T> callback)
    {
        callback(target);
        return target;
    }
}
