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

        public T NewObject<T>(SnapObj objType, int id) where T : BaseSnapObject, new()
        {
            if (_snapshotItems.Count + 1 >= MAX_SNAPSHOT_ITEMS)
            {
                Debug.Warning("snapshots", "too many items");
                return null;
            }

            var item = new SnapshotItem((int) objType << 16 | id, new T());
            _currentSize += item.Object.FieldsCount * sizeof(int);

            if (_currentSize >= MAX_SNAPSHOT_SIZE)
            {
                Debug.Warning("snapshots", "too much data");
                return null;
            }

            _snapshotItems.Add(item);
            return (T) item.Object;
        }
    }
}