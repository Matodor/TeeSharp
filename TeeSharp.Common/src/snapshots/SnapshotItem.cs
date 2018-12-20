using TeeSharp.Common.Enums;
using TeeSharp.Common.Protocol;

namespace TeeSharp.Common.Snapshots
{
    public class SnapshotItem
    {
        public SnapshotObjects Type => (SnapshotObjects) (Key >> 16);
        public int Id => Key & 0xffff;
        public readonly int Size;

        public readonly int Key;
        public readonly BaseSnapObject Object;

        public SnapshotItem(int key, int size, BaseSnapObject obj)
        {
            Size = size;
            Key = key;
            Object = obj;
        }
    }
}