using TeeSharp.Common.Enums;

namespace TeeSharp.Common.Snapshots
{
    public class SnapshotItem
    {
        public SnapshotItems Type => (SnapshotItems) (Key >> 16);
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