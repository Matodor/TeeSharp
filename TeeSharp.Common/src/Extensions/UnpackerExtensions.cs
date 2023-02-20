using TeeSharp.Common.Protocol;
using TeeSharp.Core;
using Uuids;

namespace TeeSharp.Common.Extensions;

public static class UnpackerExtensions
{
    public static bool TryGetMessageInfo(
        this Unpacker unpacker,
        out ProtocolMessage msgId,
        out Uuid msgUuid,
        out bool isSystem)
    {
        if (unpacker.HasError ||
            unpacker.TryGetInteger(out var messageInfo) == false)
        {
            msgId = default;
            isSystem = default;
            msgUuid = default;
            return false;
        }

        msgId = (ProtocolMessage)(messageInfo >> 1);
        isSystem = (messageInfo & 1) != 0;

        switch (msgId)
        {
            case < 0 or > (ProtocolMessage) ushort.MaxValue:
                msgUuid = default;
                return false;

            case ProtocolMessage.Empty:
                return unpacker.TryGetUuid(out msgUuid);

            default:
                msgUuid = default;
                return true;
        }
    }
}
