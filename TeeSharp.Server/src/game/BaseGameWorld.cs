using TeeSharp.Core;

namespace TeeSharp.Server.Game
{
    public abstract class BaseGameWorld : BaseInterface
    {
        public virtual bool IsPaused { get; set; }

        public abstract void Tick();
        public abstract void OnSnapshot(int clientId);
    }
}