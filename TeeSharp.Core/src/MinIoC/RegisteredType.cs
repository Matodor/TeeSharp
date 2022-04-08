// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
// https://github.com/microsoft/MinIoC/blob/master/Container.cs

using System;

namespace TeeSharp.Core.MinIoC;

public partial class Container
{
    private class RegisteredType : IRegisteredType
    {
        private readonly Type _itemType;
        private readonly Action<Func<ILifetime, object>> _registerFactory;
        private readonly Func<ILifetime, object> _factory;

        public RegisteredType(
            Type itemType, 
            Action<Func<ILifetime, object>> registerFactory,
            Func<ILifetime, object> factory)
        {
            _itemType = itemType;
            _registerFactory = registerFactory;
            _registerFactory(factory);
            _factory = factory;
        }

        public void AsSingleton()
        {
            _registerFactory(lifetime => lifetime.GetServiceAsSingletone(_itemType, _factory));
        }

        public void PerScope()
        {
            _registerFactory(lifetime => lifetime.GetServicePerScope(_itemType, _factory));
        }
    }
}