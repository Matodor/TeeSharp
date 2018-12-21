using TeeSharp.Common.Enums;

namespace TeeSharp.Common.Protocol
{
    public abstract class BaseGameMessage
    {
        public abstract GameMessages Type { get; }

        public abstract bool PackError(MsgPacker packer);
    }
}