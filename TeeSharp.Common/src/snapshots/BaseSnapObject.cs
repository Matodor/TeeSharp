using TeeSharp.Common.Enums;

namespace TeeSharp.Common.Snapshots
{
    public abstract class BaseSnapObject
    {
        public abstract SnapObject Type { get; }
        public abstract int SerializeLength { get; }

        public abstract void Deserialize(int[] data, int dataOffset);
        public abstract int[] Serialize();

        public bool RangeCheck(int[] data, int dataOffset)
        {
            return
                data.Length >= SerializeLength &&
                dataOffset + SerializeLength <= data.Length;
        }
    }
}