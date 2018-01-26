using TeeSharp.Common.Enums;

namespace TeeSharp.Common.Protocol
{
    public abstract class BaseGameMessage
    {
        public abstract GameMessages MsgId { get; }

        public abstract bool Pack(MsgPacker packer);
    }
}