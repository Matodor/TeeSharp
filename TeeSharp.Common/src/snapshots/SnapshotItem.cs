using TeeSharp.Common.Enums;

namespace TeeSharp.Common.Snapshots
{
    public class SnapshotItem
    {
        public readonly int Id;
        public readonly int Key;
        public readonly BaseSnapshotItem Item;

        public SnapshotItem(int id, BaseSnapshotItem item)
        {
            Key = Snapshot.Key(id, item.Type);
            Id = id;
            Item = item;
        }
    }
}