using System;
using System.Diagnostics.CodeAnalysis;

namespace TeeSharp.Network.Abstract;

public interface INetworkPacketUnpacker
{
    bool TryUnpack(Span<byte> data,
        [NotNullWhen(true)] out NetworkPacket? packet,
        out bool isSixUp,
        out SecurityToken? securityToken,
        out SecurityToken? responseToken);
}
