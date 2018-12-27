using TeeSharp.Common.Enums;
using TeeSharp.Core;
using TeeSharp.Network;

namespace TeeSharp.Common.Protocol
{
    public abstract class BaseGameMessage
    {
        protected const SanitizeType Sanitize = 
            SanitizeType.SanitizeCC | 
            SanitizeType.SkipStartWhitespaces;

        public abstract GameMessage Type { get; }

        public abstract bool PackError(MsgPacker packer);
        public abstract bool UnPackError(UnPacker unpacker, ref string failedOn);
    }
}