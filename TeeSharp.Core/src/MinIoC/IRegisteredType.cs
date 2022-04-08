// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
// https://github.com/microsoft/MinIoC/blob/master/Container.cs

namespace TeeSharp.Core.MinIoC;

public partial class Container
{
    public interface IRegisteredType
    {
        void AsSingleton();
        void PerScope();
    }
}