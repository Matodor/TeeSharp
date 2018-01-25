using TeeSharp.Common.NetObjects;
using TeeSharp.Common.Snapshots;

namespace TeeSharp.Server
{
    public class ServerClient : BaseServerClient
    {
        public override SnapRate SnapRate { get; set; }
        public override ServerClientState State { get; set; }
        public override int Latency { get; set; }

        public override string PlayerName { get; set; }
        public override string PlayerClan { get; set; }
        public override int PlayerCountry { get; set; }

        public override long TrafficSince { get; set; }
        public override long Traffic { get; set; }
        public override long LastAckedSnapshot { get; set; }
        public override long LastInputTick { get; set; }
        public override int CurrentInput { get; set; }

        public override SnapshotStorage SnapshotStorage { get; protected set; }
        public override Input[] Inputs { get; protected set; }

        public ServerClient()
        {
            State = ServerClientState.EMPTY;
            Inputs = new Input[INPUT_COUNT];
            SnapshotStorage = new SnapshotStorage();

            for (int i = 0; i < Inputs.Length; i++)
            {
                Inputs[i] = new Input()
                {
                    Tick = -1,
                    PlayerInput = new NetObj_PlayerInput()
                };
            }
        }

        public override void Reset()
        {
            PlayerName = string.Empty;
            PlayerClan = string.Empty;
            PlayerCountry = -1;

            Traffic = 0;
            TrafficSince = 0;

            for (int i = 0; i < Inputs.Length; i++)
            {
                Inputs[i].Tick = -1;
            }

            CurrentInput = 0;

            SnapshotStorage.PurgeAll();
            LastAckedSnapshot = -1;
            LastInputTick = -1;
            SnapRate = SnapRate.INIT;
        }
    }
}