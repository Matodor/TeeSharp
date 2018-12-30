using TeeSharp.Common.Enums;
using TeeSharp.Core;
using TeeSharp.Network;

namespace TeeSharp.Common.Protocol
{
    public abstract class BaseGameMsgUnpacker : BaseInterface
    {
        public virtual int MaxClients { get; set; }

        public abstract bool UnpackMessage(GameMessage msg, 
            UnPacker unPacker, out BaseGameMessage value, out string failedOn);
    }
}