using TeeSharp.Common.Protocol;
using TeeSharp.Common.Snapshots;

namespace TeeSharp.Server
{
    public class ServerClient : BaseServerClient
    {
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
                    PlayerInput = new SnapshotPlayerInput()
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