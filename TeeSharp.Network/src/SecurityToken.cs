namespace TeeSharp.Network
{
    public struct SecurityToken
    {
        private int _value;
        
        private SecurityToken(int value)
        {
            _value = value;
        }

        public static implicit operator SecurityToken(int value)
        {
            return new SecurityToken(value);
        }
    }
}