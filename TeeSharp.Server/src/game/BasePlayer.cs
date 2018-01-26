using TeeSharp.Common.Enums;
using TeeSharp.Core;

namespace TeeSharp.Server.Game
{
    public abstract class BasePlayer : BaseInterface
    {
        public abstract int ClientId { get; }

        public abstract void Init(int clientId, Team team);
    }
}