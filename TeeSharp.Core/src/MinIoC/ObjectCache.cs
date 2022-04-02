// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
// https://github.com/microsoft/MinIoC/blob/master/Container.cs

using System;
using System.Collections.Concurrent;

namespace TeeSharp.Core.MinIoC;

public partial class Container
{
    private abstract class ObjectCache
    {
        private readonly ConcurrentDictionary<Type, object> _instanceCache =
            new ConcurrentDictionary<Type, object>();

        protected object GetCached(Type type, Func<ILifetime, object> factory, ILifetime lifetime)
        {
            return _instanceCache.GetOrAdd(type, _ => factory(lifetime));
        }

        public void Dispose()
        {
            foreach (var obj in _instanceCache.Values)
            {
                (obj as IDisposable)?.Dispose();
            }
        }
    }
}