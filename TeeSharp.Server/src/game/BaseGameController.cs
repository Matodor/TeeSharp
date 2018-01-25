using TeeSharp.Core;

namespace TeeSharp.Server.Game
{
    public abstract class BaseGameController : BaseInterface
    {
        public abstract string GameType { get; }
    }
}