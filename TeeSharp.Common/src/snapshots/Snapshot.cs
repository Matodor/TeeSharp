using TeeSharp.Common.Enums;
using TeeSharp.Core;

namespace TeeSharp.Common.Snapshots
{
    public class Snapshot
    {
        public const int MAX_SNAPSHOT_PACKSIZE = 900;
        public SnapshotItem this[int index] => _items[index];

        public int ItemsCount => _items.Length;
        public int Size { get; private set; }

        private SnapshotItem[] _items;

        public Snapshot(SnapshotItem[] items, int size)
        {
            _items = items;
            Size = size;
        }

        public void Clear()
        {
            _items = new SnapshotItem[0];
            Size = 0;
        }

        public SnapshotItem FindItem(int id, SnapshotObjects type)
        {
            var key = (int) type << 16 | id;
            return FindItem(key);
        }

        public SnapshotItem FindItem(int key)
        {
            for (var i = 0; i < _items.Length; i++)
            {
                if (_items[i].Key == key)
                    return _items[i];
            }

            return null;
        }

        public int Crc()
        {
            int crc = 0;

            for (var i = 0; i < _items.Length; i++)
            {
                var data = _items[i].Object.Serialize();
                for (var field = 0; field < data.Length; field++)
                {
                    crc += data[field];
                }
            }

            return crc;
        }

        public void DebugDump()
        {
            Debug.Log("snapshot", $"data_size={Size} num_items={ItemsCount}");
            for (var i = 0; i < _items.Length; i++)
            {
                Debug.Log("snapshot", $"type={_items[i].Type} id={_items[i].Id}");
                var data = _items[i].Object.Serialize();
                for (var field = 0; field < data.Length; field++)
                    Debug.Log("snapshot", $"field={field} value={data[field]}");
            }
        }
    }
}