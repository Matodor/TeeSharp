using System;
using System.Diagnostics.CodeAnalysis;

namespace TeeSharp.Network.Abstract;

public interface INetworkPacketUnpacker
{
    bool TryUnpack(Span<byte> data,
        [NotNullWhen(true)] out NetworkPacket? packet,
        ref bool isSixUp,
        ref SecurityToken securityToken,
        ref SecurityToken responseToken);
}
