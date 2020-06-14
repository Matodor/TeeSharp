using System.Collections.Generic;

namespace TeeSharp.Common.Snapshots
{
    public class SnapshotStorage
    {
        protected IList<SnapshotInfo> SnapshotInfos { get; set; }

        public SnapshotStorage()
        {
            SnapshotInfos = new List<SnapshotInfo>();
        }

        public void PurgeUntil(int tick)
        {
            for (var i = 0; i < SnapshotInfos.Count; i++)
            {
                if (SnapshotInfos[i].Tick >= tick)
                    return;

                SnapshotInfos.RemoveAt(i--);
            }
        }

        public void Add(int tick, long tagTime, Snapshot snapshot)
        {
            SnapshotInfos.Add(new SnapshotInfo
            {
                Tick = tick,
                TagTime = tagTime,
                Snapshot = snapshot,
            });
        }

        public bool Get(int tick, out long tagTime, out Snapshot snapshot)
        {
            for (var i = 0; i < SnapshotInfos.Count; i++)
            {
                if (SnapshotInfos[i].Tick == tick)
                {
                    tagTime = SnapshotInfos[i].TagTime;
                    snapshot = SnapshotInfos[i].Snapshot;
                    return true;
                }
            }
            tagTime = -1;
            snapshot = null;
            return false;
        }

        public void PurgeAll()
        {
            SnapshotInfos.Clear();
        }
    }
}