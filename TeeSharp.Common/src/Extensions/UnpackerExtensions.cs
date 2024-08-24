using TeeSharp.Core;
using Uuids;

namespace TeeSharp.Common.Extensions;

public static class UnpackerExtensions
{
    public static bool TryGetMessageInfo(
        this ref Unpacker unpacker,
        out Protocol.Message message,
        out Uuid messageExtended,
        out bool isSystem)
    {
        if (unpacker.HasError ||
            unpacker.TryGetInteger(out var messageInfo) == false)
        {
            message = default;
            isSystem = default;
            messageExtended = default;
            return false;
        }

        message = (Protocol.Message)(messageInfo >> 1);
        isSystem = (messageInfo & 1) != 0;

        switch (message)
        {
            case < 0 or > (Protocol.Message) ushort.MaxValue:
                messageExtended = default;
                return false;

            case Protocol.Message.Empty:
                return unpacker.TryGetUuid(out messageExtended);

            default:
                messageExtended = default;
                return true;
        }
    }
}
