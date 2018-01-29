using TeeSharp.Common.Enums;

namespace TeeSharp.Common.Protocol
{
    public abstract class BaseSnapObject
    {
        public abstract SnapObj Type { get; }
        public abstract int SerializeLength { get; }

        public abstract int[] Serialize();
    }
}