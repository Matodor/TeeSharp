using TeeSharp.Core;

namespace TeeSharp.Common.Snapshots
{
    public class Snapshot
    {
        public const int MAX_SNAPSHOT_PACKSIZE = 900;

        public SnapshotItem this[int index] => _items[index];

        public readonly int ItemsCount;
        public readonly int Size;
        
        private readonly SnapshotItem[] _items;

        public Snapshot(SnapshotItem[] items, int size)
        {
            _items = items;
            ItemsCount = _items.Length;
            Size = size;
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