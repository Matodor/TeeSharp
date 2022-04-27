using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace TeeSharp.Network;

public readonly struct SecurityToken : IEquatable<SecurityToken>
{
    public static readonly SecurityToken Unknown = -1;
    public static readonly SecurityToken Unsupported = 0;
    public static readonly SecurityToken Magic = BitConverter.ToInt32(new []
    {
        (byte) 'T',
        (byte) 'K',
        (byte) 'E',
        (byte) 'N',
    });

    private readonly int _value;

    private SecurityToken(int value)
    {
        _value = value;
    }

    public void CopyTo(Span<byte> buffer)
    {
        Unsafe.As<byte, int>(ref MemoryMarshal.GetReference(buffer)) = _value;
    }

    public static implicit operator SecurityToken(int value)
    {
        return new SecurityToken(value);
    }

    public static explicit operator SecurityToken(byte[] data)
    {
        return new SecurityToken(BitConverter.ToInt32(data, 0));
    }

    public static explicit operator SecurityToken(Span<byte> data)
    {
        return new SecurityToken(BitConverter.ToInt32(data));
    }

    public static implicit operator byte[](SecurityToken token)
    {
        return BitConverter.GetBytes(token._value);
    }

    public bool Equals(SecurityToken other)
    {
        return _value == other._value;
    }

    public override bool Equals(object? obj)
    {
        return obj is SecurityToken other && Equals(other);
    }

    public override int GetHashCode()
    {
        return _value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator ==(Span<byte> left, SecurityToken right)
    {
        return BitConverter.ToInt32(left) == right._value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator !=(Span<byte> left, SecurityToken right)
    {
        return !(left == right);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator ==(SecurityToken left, Span<byte> right)
    {
        return right == left;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator !=(SecurityToken left, Span<byte> right)
    {
        return !(right == left);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator ==(SecurityToken left, SecurityToken right)
    {
        return left.Equals(right);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator !=(SecurityToken left, SecurityToken right)
    {
        return !left.Equals(right);
    }
}
