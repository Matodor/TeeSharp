// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
// https://github.com/microsoft/MinIoC/blob/master/Container.cs

using System;

namespace TeeSharp.Core.MinIoC;

public partial class Container
{
    private interface ILifetime : IScope
    {
        object GetServiceAsSingletone(Type type, Func<ILifetime, object> factory);
        object GetServicePerScope(Type type, Func<ILifetime, object> factory);
    }
}