using System;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using TeeSharp.Core;
using Uuids;

namespace TeeSharp.Common.Protocol;

public ref struct UnPacker
{
    public bool HasError { get; private set; }

    public readonly Span<byte> DataOriginal;

    private Span<byte> _data;

    public UnPacker(byte[] data)
    {
        _data = data;
        DataOriginal = _data;
        HasError = false;
    }

    public bool TryGetMessageInfo(out ProtocolMessage msgId, out Uuid msgUuid, out bool isSystem)
    {
        if (HasError || !TryGetInteger(out var messageInfo))
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
                return TryGetUuid(out msgUuid);

            default:
                msgUuid = default;
                return true;
        }
    }

    public bool TryGetUuid(out Uuid result)
    {
        if (TryGetRaw(16, out var uuidData))
        {
            result = new Uuid(uuidData);
            return true;
        }

        result = default;
        return false;
    }

    public bool TryGetInteger(out int result)
    {
        if (!HasError && CompressionableInt.TryUnpack(_data, out result, out _data))
            return true;

        HasError = true;
        result = default;
        return false;
    }

    public bool TryGetRaw(int size, out Span<byte> result)
    {
        if (HasError || size < 0 || size > _data.Length)
        {
            HasError = true;
            result = default;
            return false;
        }

        result = _data.Slice(0, size);
        _data = _data.Slice(size);
        return true;
    }

    public bool TryGetString([NotNullWhen(true)] out string? result)
    {
        if (HasError)
        {
            HasError = true;
            result = default;
            return false;
        }

        var endIndex = _data.IndexOf((byte)0);
        if (endIndex == -1)
        {
            result = Encoding.UTF8.GetString(_data);
            _data = Span<byte>.Empty;
        }
        else
        {
            result = Encoding.UTF8.GetString(_data.Slice(0, endIndex));
            _data = _data.Slice(endIndex + 1);
        }

        return true;
    }
}
