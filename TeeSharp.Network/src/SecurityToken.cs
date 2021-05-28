using System;

namespace TeeSharp.Network
{
    public readonly struct SecurityToken : IEquatable<SecurityToken>
    {
        public static readonly SecurityToken TokenUnknown = -1;
        public static readonly SecurityToken TokenUnsupported = 0;
        
        private readonly int _value;
        
        private SecurityToken(int value)
        {
            _value = value;
        }

        public static implicit operator SecurityToken(int value)
        {
            return new SecurityToken(value);
        }

        public static implicit operator SecurityToken(byte[] data)
        {
            // ReSharper disable once ArrangeRedundantParentheses
            return new SecurityToken(
                (data[0]) |
                (data[1] << 8) |
                (data[2] << 16) |
                (data[3] << 24)
            );
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

        public static bool operator ==(SecurityToken left, SecurityToken right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(SecurityToken left, SecurityToken right)
        {
            return !left.Equals(right);
        }
        
        public static SecurityToken operator |(SecurityToken left, SecurityToken right)
        {
            return new SecurityToken(left._value | right._value);
        }
        
        public static SecurityToken operator |(SecurityToken left, int right)
        {
            return new SecurityToken(left._value | right);
        }
        
        public static SecurityToken operator |(SecurityToken left, byte right)
        {
            return new SecurityToken(left._value | right);
        }
        
        public static SecurityToken operator |(int left, SecurityToken right)
        {
            return new SecurityToken(left | right._value);
        }
        
        public static SecurityToken operator |(byte left, SecurityToken right)
        {
            return new SecurityToken(left | right._value);
        }
    }
}