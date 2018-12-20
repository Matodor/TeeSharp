using System.Collections.Generic;
using System.Linq;
using TeeSharp.Common.Enums;
using TeeSharp.Common.Protocol;
using TeeSharp.Core;

namespace TeeSharp.Common.Snapshots
{
    public class SnapshotBuilder
    {
        public const int MAX_SNAPSHOT_SIZE = 65536;
        public const int MAX_SNAPSHOT_ITEMS = 1024;

        private readonly IList<SnapshotItem> _snapshotItems;
        private int _currentSize;

        public SnapshotBuilder()
        {
            _snapshotItems = new List<SnapshotItem>(MAX_SNAPSHOT_ITEMS);
        }

        public void StartBuild()
        {
            _currentSize = 0;
            _snapshotItems.Clear();
        }

        public Snapshot EndBuild()
        {
            return new Snapshot(_snapshotItems.ToArray(), _currentSize);
        }

        public SnapshotItem FindItem(int key)
        {
            for (var i = 0; i < _snapshotItems.Count; i++)
            {
                if (_snapshotItems[i].Key == key)
                    return _snapshotItems[i];
            }
            return null;
        }

        public bool AddItem<T>(T obj, int id) where T : BaseSnapObject
        {
            if (obj == null)
            {
                Debug.Warning("snapshots", "add null object");
                return false;
            }

            if (_snapshotItems.Count + 1 >= MAX_SNAPSHOT_ITEMS)
            {
                Debug.Warning("snapshots", "too many items");
                return false;
            }

            if (obj.Type <= Enums.SnapshotObjects.INVALID || 
                obj.Type >= Enums.SnapshotObjects.NUM)
            {
                Debug.Warning("snapshots", "wrong object type");
                return false;
            }
            
            var itemSize = obj.SerializeLength * sizeof(int);
            if (_currentSize + itemSize >= MAX_SNAPSHOT_SIZE)
            {
                Debug.Warning("snapshots", "too much data");
                return false;
            }

            var item = new SnapshotItem((int)obj.Type << 16 | id, itemSize, obj);
            _currentSize += itemSize;
            _snapshotItems.Add(item);
            return true;
        }

        public T NewObject<T>(int id) where T : BaseSnapObject, new()
        {
            if (_snapshotItems.Count + 1 >= MAX_SNAPSHOT_ITEMS)
            {
                Debug.Warning("snapshots", "too many items");
                return null;
            }
            
            var obj = new T();
            if (obj.Type <= Enums.SnapshotObjects.INVALID || 
                obj.Type >= Enums.SnapshotObjects.NUM)
            {
                Debug.Warning("snapshots", "wrong object type");
                return null;
            }

            var itemSize = obj.SerializeLength * sizeof(int);
            if (_currentSize + itemSize >= MAX_SNAPSHOT_SIZE)
            {
                Debug.Warning("snapshots", "too much data");
                return null;
            }

            var item = new SnapshotItem((int)obj.Type << 16 | id, itemSize, obj);
            _currentSize += itemSize;
            _snapshotItems.Add(item);
            return (T) item.Object;
        }
    }
}