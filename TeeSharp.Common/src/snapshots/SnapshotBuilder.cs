using System.Collections.Generic;
using System.Linq;
using TeeSharp.Core;

namespace TeeSharp.Common.Snapshots
{
    public class SnapshotBuilder
    {
        public const int MaxItems = 1024;

        protected virtual IList<SnapshotItem> SnapshotItems { get; set; }
        protected virtual int SnapshotSize { get; set; }

        public SnapshotBuilder()
        {
            SnapshotItems = new List<SnapshotItem>(MaxItems);
            SnapshotSize = 0;
        }

        public void Start()
        {
            SnapshotItems.Clear();
            SnapshotSize = 0;
        }

        public Snapshot Finish()
        {
            return new Snapshot(SnapshotItems.ToArray(), SnapshotSize);
        }

        public SnapshotItem FindItem(int key)
        {
            for (var i = 0; i < SnapshotItems.Count; i++)
            {
                if (SnapshotItems[i].Key == key)
                    return SnapshotItems[i];
            }
            return null;
        }

        public bool AddItem<T>(T obj, int id) where T : BaseSnapshotItem
        {
            if (obj == null)
            {
                Debug.Warning("snapshots", "add null object");
                return false;
            }

            if (SnapshotItems.Count + 1 >= MaxItems)
            {
                Debug.Warning("snapshots", "too many items");
                return false;
            }

            var itemSize = SnapshotItemsInfo.GetSize<T>();
            if (SnapshotSize + itemSize >= Snapshot.MaxSize)
            {
                Debug.Warning("snapshots", "too much data");
                return false;
            }

            var item = new SnapshotItem(id, obj);
            SnapshotSize += itemSize;
            SnapshotItems.Add(item);
            return true;
        }

        public T NewItem<T>(int id) where T : BaseSnapshotItem, new()
        {
            if (SnapshotItems.Count + 1 >= MaxItems)
            {
                Debug.Warning("snapshots", "too many items");
                return null;
            }

            var itemSize = SnapshotItemsInfo.GetSize<T>();

            if (SnapshotSize + itemSize >= Snapshot.MaxSize)
            {
                Debug.Warning("snapshots", "too much data");
                return null;
            }

            var item = new SnapshotItem(id, new T());
            SnapshotSize += itemSize;
            SnapshotItems.Add(item);
            return (T) item.Item;
        }
    }
}