using System.Collections;
using System.Collections.Generic;

namespace TeeSharp.Common.Snapshots
{
    public class SnapshotInfo
    {
        public long TagTime;
        public long Tick;
        public Snapshot Snapshot;
    }

    public class SnapshotStorage
    {
        private readonly IList<SnapshotInfo> _snapshots;

        public SnapshotStorage()
        {
            _snapshots = new List<SnapshotInfo>();
        }

        public void PurgeUntil(long tick)
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

        public void Add(long tick, long tagTime, Snapshot snapshot)
        {
            _snapshots.Add(new SnapshotInfo
            {
                Tick = tick,
                TagTime = tagTime,
                Snapshot = snapshot
            });
        }

        public bool Get(long tick, out long tagTime, out Snapshot snapshot)
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