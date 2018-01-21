using TeeSharp.Common.Snapshots;

namespace TeeSharp.Server
{
    public class ServerClient : BaseServerClient
    {
        public override ServerClientState State { get; set; }
        public override string PlayerName { get; set; }
        public override string PlayerClan { get; set; }
        public override int PlayerCountry { get; set; }
        public override long TrafficSince { get; set; }
        public override long Traffic { get; set; }
        public override SnapshotStorage SnapshotStorage { get; protected set; }
        public override Input[] Inputs { get; protected set; }

        public ServerClient()
        {
            Inputs = new Input[200];
            SnapshotStorage = new SnapshotStorage();
        }

        public override void Reset()
        {
        }
    }
}