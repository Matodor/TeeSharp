using System.Net.Sockets;
using TeeSharp.Core;
using TeeSharp.Network;

namespace TeeSharp.Server
{
    public abstract class BaseRegister : BaseInterface
    {
        public abstract void RegisterUpdate(AddressFamily netType);
        public abstract bool RegisterProcessPacket(Chunk packet, uint token);
    }
}