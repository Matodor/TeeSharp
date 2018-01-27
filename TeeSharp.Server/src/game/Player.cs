using TeeSharp.Common.Enums;
using TeeSharp.Common.Protocol;

namespace TeeSharp.Server.Game
{
    public class Player : BasePlayer
    {
        public override int ClientId => _clientId;

        private int _clientId;

        public override void Init(int clientId, Team startTeam)
        {
            _clientId = clientId;

            Team = startTeam;
            TeeInfo = new TeeInfo();
            IsReady = false;
            LastChangeInfo = -1;
        }

        public override void Respawn()
        {
        }

        public override void OnPredictedInput(SnapObj_PlayerInput input)
        {
        }

        public override void OnDirectInput(SnapObj_PlayerInput input)
        {
        }
    }
}