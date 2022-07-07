using System;
using System.Diagnostics.CodeAnalysis;

namespace TeeSharp.Network.Abstract;

public interface INetworkPacketUnpacker
{
    bool TryUnpack(Span<byte> buffer, [NotNullWhen(true)] out NetworkPacketIn? packet);
}
