using TeeSharp.Common.Enums;
using TeeSharp.Core;

namespace TeeSharp.Common.Protocol
{
    public abstract class BaseGameMsgUnpacker : BaseInterface
    {
        public abstract bool UnpackMessage(GameMessage msg, 
            UnPacker unPacker, out BaseGameMessage value, out string failedOn);
    }
}