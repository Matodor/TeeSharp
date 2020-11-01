// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
// https://github.com/microsoft/MinIoC/blob/master/Container.cs

using System;

namespace TeeSharp.Core.MinIoC
{
    public partial class Container
    {
        private class ScopeLifetime : ObjectCache, ILifetime
        {
            private readonly ContainerLifetime _parentLifetime;

            public ScopeLifetime(ContainerLifetime parent)
            {
                _parentLifetime = parent;
            }

            public object GetService(Type type)
            {
                return _parentLifetime.GetFactory(type)(this);
            }

            public object GetServiceAsSingletone(Type type, Func<ILifetime, object> factory)
            {
                return _parentLifetime.GetServiceAsSingletone(type, factory);
            }

            public object GetServicePerScope(Type type, Func<ILifetime, object> factory)
            {
                return GetCached(type, factory, this);
            }
        }
    }
}
