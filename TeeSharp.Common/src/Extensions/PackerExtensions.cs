using TeeSharp.Core;
using Uuids;

namespace TeeSharp.Common.Extensions;

public static class PackerExtensions
{
    public static Packer AddProtocolMessage(this Packer packer, Protocol.Message msgId)
    {
        packer.AddInteger((int) msgId << 1 | 1);
        return packer;
    }

    public static Packer AddProtocolMessageExtended(this Packer packer, Uuid msgId, bool isSystem)
    {
        packer.AddInteger(isSystem ? 1 : 0);
        packer.AddUuid(msgId);
        return packer;
    }
}
