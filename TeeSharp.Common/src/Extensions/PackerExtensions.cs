using TeeSharp.Common.Protocol;
using TeeSharp.Core;

namespace TeeSharp.Common.Extensions;

public static class PackerExtensions
{
    public static Packer AddProtocolMessage(this Packer packer, ProtocolMessage msgId)
    {
        packer.AddInteger((int) msgId << 1 | 1);
        return packer;
    }
}
