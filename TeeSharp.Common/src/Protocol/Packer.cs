using System;
using System.Text;
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

    // public Packer(GameMessage msgId) : this()
    // {
    //     AddInteger((int) msgId << 1);
    // }

    public Packer(ProtocolMessage msgId) : this()
    {
        AddInteger((int) msgId << 1 | 1);
    }

    public Packer(Uuid msgUuid, bool isSystem) : this()
    {
        AddInteger(isSystem ? 1 : 0);
        AddUuid(msgUuid);
    }

    public void AddBoolean(bool value)
    {
        AddInteger(value ? 1 : 0);
    }

    public void AddInteger(int value)
    {
        if (HasError)
            return;

        if (CompressionableInt.TryPack(_buffer, value, ref _bufferIndex))
            return;

        HasError = true;
    }

    public void AddUuid(Uuid uuid)
    {
        if (HasError)
            return;

        if (uuid.TryWriteBytes(_buffer.Slice(_bufferIndex)))
        {
            _bufferIndex += StructHelper<Uuid>.Size;
            return;
        }

        HasError = true;
    }

    public void AddRaw(Span<byte> data)
    {
        if (HasError)
            return;

        if (data.Length + _bufferIndex > _buffer.Length)
        {
            HasError = true;
            return;
        }

        data.CopyTo(_buffer.Slice(_bufferIndex));
        _bufferIndex += data.Length;
    }

    public void AddString(string str)
    {
        if (HasError)
            return;

        if (str == null!)
        {
            HasError = true;
            return;
        }

        var strBytes = Encoding.UTF8.GetBytes(str);
        if (strBytes.Length + _bufferIndex > _buffer.Length)
        {
            HasError = true;
            return;
        }

        strBytes.CopyTo(_buffer.Slice(_bufferIndex));
        _bufferIndex += strBytes.Length;
    }
}
