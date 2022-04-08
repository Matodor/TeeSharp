// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
// https://github.com/microsoft/MinIoC/blob/master/Container.cs

using System;

namespace TeeSharp.Core.MinIoC;

public partial class Container
{
    private class ContainerLifetime : ObjectCache, ILifetime
    {
        public Func<Type, Func<ILifetime, object>> GetFactory { get; private set; }

        public ContainerLifetime(Func<Type, Func<ILifetime, object>> getFactory)
        {
            GetFactory = getFactory;
        }

        public object GetService(Type type)
        {
            return GetFactory(type)(this);
        }

        public object GetServiceAsSingletone(Type type, Func<ILifetime, object> factory)
        {
            return GetCached(type, factory, this);
        }

        public object GetServicePerScope(Type type, Func<ILifetime, object> factory)
        {
            return GetServiceAsSingletone(type, factory);
        }
    }
}