using TeeSharp.Common;
using TeeSharp.Common.Enums;
using TeeSharp.Core;

namespace TeeSharp.Server.Game
{
    public abstract class BaseGameController : BaseInterface
    {
        public abstract string GameType { get; }

        public abstract Team GetAutoTeam(int clientId);
        public abstract bool CheckTeamsBalance();

        public abstract void OnEntity(int tileIndex, Vector2 pos);
        public abstract void OnPlayerInfoChange(BasePlayer player);
    }
}