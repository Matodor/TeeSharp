using TeeSharp.Core;

namespace TeeSharp.Common.Protocol
{
    public abstract class BaseGameMsgUnpacker : BaseInterface
    {
        public abstract bool Unpack(int msgId, Unpacker unpacker, 
            out BaseGameMessage msg, out string error);
    }
}