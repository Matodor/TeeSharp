using System;
using TeeSharp.Core.MinIoC;

namespace TeeSharp.Network
{
    public abstract class BaseNetworkConnection : IContainerService
    {
        public Container Container { get; set; }
    }
}