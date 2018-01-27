using TeeSharp.Common.Protocol;

namespace TeeSharp.Common.Snapshots
{
    public class SnapshotItem
    {
        public int Type => TypeAndID >> 16;
        public int Id => TypeAndID & 0xffff;
        public int Key => TypeAndID;

        public readonly int TypeAndID;
        public readonly BaseSnapObject Object;

        public SnapshotItem(int typeAndId, BaseSnapObject obj)
        {
            TypeAndID = typeAndId;
            Object = obj;
        }
    }
}