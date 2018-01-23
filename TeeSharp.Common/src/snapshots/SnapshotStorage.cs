namespace TeeSharp.Common.Snapshots
{
    public class SnapshotStorage
    {
        public void PurgeUntil(long tick)
        {
            throw new System.NotImplementedException();
        }

        public void Add(long tick, long tagTime, Snapshot snapshot)
        {
            throw new System.NotImplementedException();
        }

        public bool Get(long tick, out long tagTime, out Snapshot snapshot)
        {
            throw new System.NotImplementedException();
        }

        public void PurgeAll()
        {
            throw new System.NotImplementedException();
        }
    }
}