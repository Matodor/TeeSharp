using System.Net.Sockets;
using TeeSharp.Core;

namespace TeeSharp.Server
{
    public abstract class BaseRegister : BaseInterface
    {
        public abstract void RegisterUpdate(AddressFamily netType);
    }
}