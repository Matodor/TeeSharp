using System;
using System.Collections.Generic;
using TeeSharp.Core.Helpers;

namespace TeeSharp.Common.Commands
{
    public static class ReadersContainer
    {
        private static readonly Dictionary<Type, IArgumentReader> _instances;

        static ReadersContainer()
        {
            _instances = new Dictionary<Type, IArgumentReader>();
        }
        
        public static IArgumentReader GetInstance<T>() where T : IArgumentReader, new()
        {
            lock (_instances)
            {
                if (!_instances.TryGetValue(TypeHelper<T>.Type, out var instance))
                    _instances.Add(TypeHelper<T>.Type, instance = new T());
                
                return instance;
            }
        }
    }
}