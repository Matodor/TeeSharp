using TeeSharp.Common.Snapshots;

namespace TeeSharp.Server
{
    public class ServerClient : BaseServerClient
    {
        public override SnapRate SnapRate { get; set; }
        public override ServerClientState State { get; set; }

        public override string PlayerName { get; set; }
        public override string PlayerClan { get; set; }
        public override int PlayerCountry { get; set; }

        public override long TrafficSince { get; set; }
        public override long Traffic { get; set; }
        public override long LastAckedSnapshot { get; set; }
        public override SnapshotStorage SnapshotStorage { get; protected set; }
        public override Input[] Inputs { get; protected set; }

        public ServerClient()
        {
            State = ServerClientState.EMPTY;
            Inputs = new Input[INPUT_COUNT];
            SnapshotStorage = new SnapshotStorage();

            for (int i = 0; i < Inputs.Length; i++)
            {
                Inputs[i] = new Input(new int[MAX_INPUT_SIZE], -1);
            }
        }

        public override void Reset()
        {
            for (int i = 0; i < Inputs.Length; i++)
            {
                Inputs[i].Tick = -1;
            }

            SnapshotStorage.PurgeAll();
            LastAckedSnapshot = -1;
            SnapRate = SnapRate.INIT;
        }
    }
}