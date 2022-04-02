using System;
using System.Collections.Generic;
using TeeSharp.Core.Helpers;

namespace TeeSharp.Commands;

public static class ReadersContainer
{
    private static readonly Dictionary<Type, IArgumentReader> _instances;

    static ReadersContainer()
    {
        _instances = new Dictionary<Type, IArgumentReader>();
    }
        
    public static IArgumentReader GetInstance<T>() where T : class, IArgumentReader, new()
    {
        lock (_instances)
        {
            if (!_instances.TryGetValue(ClassHelper<T>.Type, out var instance))
                _instances.Add(ClassHelper<T>.Type, instance = new T());
                
            return instance;
        }
    }
}