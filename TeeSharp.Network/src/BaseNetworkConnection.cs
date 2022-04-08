using System;
using TeeSharp.Core.MinIoC;

namespace TeeSharp.Network;

public abstract class BaseNetworkConnection : IContainerService
{
    public Container.IScope Container { get; set; }
    public bool IsSixUp { get; set; }
}