using System.Collections.Generic;

namespace TeeSharp.Common.Snapshots
{
    public class SnapshotInfo
    {
        public long TagTime;
        public int Tick;
        public Snapshot Snapshot;
    }

    public class SnapshotStorage
    {
        public SnapshotInfo this[int index] => _snapshots[index];

        private readonly IList<SnapshotInfo> _snapshots;

        public SnapshotStorage()
        {
            _snapshots = new List<SnapshotInfo>();
        }

        public void PurgeUntil(int tick)
        {
            for (var i = 0; i < _snapshots.Count; i++)
            {
                if (_snapshots[i].Tick >= tick)
                    return;

                _snapshots.RemoveAt(i);
                i--;
            }
            _snapshots.Clear();
        }

        public void Add(int tick, long tagTime, Snapshot snapshot)
        {
            _snapshots.Add(new SnapshotInfo
            {
                Tick = tick,
                TagTime = tagTime,
                Snapshot = snapshot
            });
        }

        public bool Get(int tick, out long tagTime, out Snapshot snapshot)
        {
            for (var i = 0; i < _snapshots.Count; i++)
            {
                if (_snapshots[i].Tick == tick)
                {
                    tagTime = _snapshots[i].TagTime;
                    snapshot = _snapshots[i].Snapshot;
                    return true;
                }
            }

            tagTime = -1;
            snapshot = null;
            return false;
        }

        public void PurgeAll()
        {
            _snapshots.Clear();
        }
    }
}