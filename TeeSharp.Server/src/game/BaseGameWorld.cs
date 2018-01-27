using TeeSharp.Core;

namespace TeeSharp.Server.Game
{
    public abstract class BaseGameWorld : BaseInterface
    {
        public virtual bool IsPaused { get; set; }
    }
}