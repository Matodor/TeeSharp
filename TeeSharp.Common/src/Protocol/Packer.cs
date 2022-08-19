using System;
using TeeSharp.Core;
using TeeSharp.Core.Helpers;
using Uuids;

namespace TeeSharp.Common.Protocol;

public ref struct Packer
{
    public const int BufferSize = 2048;

    public Span<byte> Buffer => _buffer.Slice(0, _bufferIndex);
    public bool HasError { get; private set; }

    private readonly Span<byte> _buffer;
    private int _bufferIndex;

    public Packer()
    {
        HasError = false;

        _buffer = new byte[BufferSize];
        _bufferIndex = 0;
    }

    public Packer(ProtocolMessage msgId, bool isSystem)
    {
        HasError = false;

        _buffer = new byte[BufferSize];
        _bufferIndex = 0;

        AddInt((int) msgId << 1 | (isSystem ? 1 : 0));
    }

    public Packer(Uuid msgUuid, bool isSystem)
    {
        HasError = false;

        _buffer = new byte[BufferSize];
        _bufferIndex = 0;

        AddInt(isSystem ? 1 : 0);
        AddUuid(msgUuid);
    }

    public void AddInt(int value)
    {
        if (!HasError && CompressionableInt.TryPack(_buffer, value, ref _bufferIndex))
            return;

        HasError = true;
    }

    public void AddUuid(Uuid uuid)
    {
        if (uuid.TryWriteBytes(_buffer.Slice(_bufferIndex)))
        {
            _bufferIndex += StructHelper<Uuid>.Size;
            return;
        }

        HasError = true;
    }

    public void AddRaw(Span<byte> data)
    {
        if (!HasError && _bufferIndex + data.Length <= _buffer.Length)
        {
            data.CopyTo(_buffer.Slice(_bufferIndex));
            _bufferIndex += data.Length;
            return;
        }

        HasError = true;
    }
}
