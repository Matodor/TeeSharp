namespace TeeSharp.Common.Snapshots
{
    public class SnapshotInfo
    {
        public long TagTime;
        public int Tick;
        public Snapshot Snapshot;
        public SnapshotInfo Previous;
        public SnapshotInfo Next;
    }

    public class SnapshotStorage
    {
        public SnapshotInfo First { get; private set; }
        public SnapshotInfo Last { get; private set; }

        public SnapshotStorage()
        {
            First = null;
            Last = null;
        }

        public void PurgeUntil(int tick)
        {
            var holder = First;

            while (holder != null)
            {
                var next = holder.Next;

                if (holder.Tick >= tick)
                    return;

                holder.Next = null;
                holder.Previous = null;

                if (next == null)
                    break;

                First = next;
                next.Previous = null;
                holder = next;
            }

            First = null;
            Last = null;
        }

        public void Add(int tick, long tagTime, Snapshot snapshot)
        {
            var holder = new SnapshotInfo
            {
                Tick = tick,
                TagTime = tagTime,
                Snapshot = snapshot,
                Next = null,
                Previous = null
            };

            holder.Next = null;
            holder.Previous = Last;

            if (Last != null)
                Last.Next = holder;
            else
                First = holder;
            Last = holder;
        }

        public bool Get(int tick, out long tagTime, out Snapshot snapshot)
        {
            var holder = First;

            while (holder != null)
            {
                if (holder.Tick == tick)
                {
                    tagTime = holder.TagTime;
                    snapshot = holder.Snapshot;
                    return true;
                }

                holder = holder.Next;
            }

            tagTime = -1;
            snapshot = null;
            return false;
        }

        public void PurgeAll()
        {
            var holder = First;

            while (holder != null)
            {
                var next = holder.Next;
                holder.Previous = null;
                holder.Next = null;
                holder = next;
            }
            
            First = null;
            Last = null;
        }
    }
}