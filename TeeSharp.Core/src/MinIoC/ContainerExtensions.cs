// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
// https://github.com/microsoft/MinIoC/blob/master/Container.cs

using System;

namespace TeeSharp.Core.MinIoC
{
    public static class ContainerExtensions
    {
        public static Container.IRegisteredType Register<T>(this Container container, Type type)
        {
            return container.Register(typeof(T), type);
        }

        public static Container.IRegisteredType Register<TInterface, TImplementation>(this Container container)
            where TImplementation : TInterface
        {
            return container.Register(typeof(TInterface), typeof(TImplementation));
        }

        public static Container.IRegisteredType Register<T>(this Container container, Func<T> factory)
        {
            return container.Register(typeof(T), () => factory());
        }

        public static Container.IRegisteredType Register<T>(this Container container)
        {
            return container.Register(typeof(T), typeof(T));
        }

        public static T Resolve<T>(this Container.IScope scope)
        {
            return (T) scope.GetService(typeof(T));
        }
    }
}