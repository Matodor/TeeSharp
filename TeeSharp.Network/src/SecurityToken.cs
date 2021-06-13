using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace TeeSharp.Network
{
    // TODO: make union trick
    public readonly struct SecurityToken : IEquatable<SecurityToken>
    {
        public static readonly SecurityToken Unknown = -1;
        public static readonly SecurityToken Unsupported = 0;
        public static readonly SecurityToken Magic = BitConverter.ToInt32(new []
        {
            (byte) 'T', (byte) 'K', (byte) 'E', (byte) 'N',
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
        
        public static implicit operator Span<byte>(SecurityToken token)
        {
            return BitConverter.GetBytes(token._value);
        }
        
        public static implicit operator SecurityToken(int value)
        {
            return new SecurityToken(value);
        }
        
        public static implicit operator SecurityToken(Span<byte> data)
        {
            return new SecurityToken(BitConverter.ToInt32(data));
        }

        public bool Equals(SecurityToken other)
        {
            return _value == other._value;
        }

        public override bool Equals(object obj)
        {
            return obj is SecurityToken other && Equals(other);
        }

        public override int GetHashCode()
        {
            return _value;
        }

        public static bool operator ==(Span<byte> left, SecurityToken right)
        {
            return BitConverter.ToInt32(left) == right._value;
        }
        
        public static bool operator !=(Span<byte> left, SecurityToken right)
        {
            return !(left == right);
        }
        
        public static bool operator ==(SecurityToken left, SecurityToken right)
        {
            return left.Equals(right);
        }
        
        public static bool operator !=(SecurityToken left, SecurityToken right)
        {
            return !left.Equals(right);
        }
        
        // public static SecurityToken operator |(SecurityToken left, SecurityToken right)
        // {
        //     return new SecurityToken(left._value | right._value);
        // }
        //
        // public static SecurityToken operator |(SecurityToken left, int right)
        // {
        //     return new SecurityToken(left._value | right);
        // }
        //
        // public static SecurityToken operator |(SecurityToken left, byte right)
        // {
        //     return new SecurityToken(left._value | right);
        // }
        //
        // public static SecurityToken operator |(int left, SecurityToken right)
        // {
        //     return new SecurityToken(left | right._value);
        // }
        //
        // public static SecurityToken operator |(byte left, SecurityToken right)
        // {
        //     return new SecurityToken(left | right._value);
        // }
    }
}