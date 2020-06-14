namespace TeeSharp.Common.Snapshots
{
    public struct SnapshotInfo
    {
        public long TagTime { get; set; }
        public int Tick { get; set; }
        public Snapshot Snapshot { get; set; }
    }
}