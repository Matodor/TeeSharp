namespace TeeSharp
{
    public class SnapshotStorage
    {
        public void Init()
        {
            
        }

        public int Get(long tick, out long tagTime, out Snapshot data)
        {
            tagTime = 0;
            data = null;
            return -1;
        }

        public void PurgeAll()
        {
            
        }

        public void PurgeUntil(long tick)
        {
            
        }
    }
}