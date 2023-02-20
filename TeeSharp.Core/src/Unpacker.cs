using System;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using Uuids;

namespace TeeSharp.Core;

public ref struct Unpacker
{
    public bool HasError { get; private set; }

    public readonly Span<byte> DataOriginal;

    private Span<byte> _data;

    public Unpacker(Span<byte> data)
    {
        _data = data;
        DataOriginal = _data;
        HasError = false;
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

    public bool TryGetBoolean(out bool result)
    {
        if (!TryGetInteger(out var resultInt) || resultInt is < 0 or > 1)
        {
            result = default;
            return false;
        }

        result = resultInt != 0;
        return true;
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
