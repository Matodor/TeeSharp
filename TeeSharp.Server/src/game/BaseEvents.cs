using System.Collections.Generic;
using TeeSharp.Common.Snapshots;
using TeeSharp.Core;

namespace TeeSharp.Server.Game
{
    public abstract class BaseEvents : BaseInterface
    {
        public struct EventInfo
        {
            public BaseSnapEvent EventItem;
            public int Mask;
        }

        protected abstract IList<EventInfo> EventInfos { get; set; }
        protected virtual int MaxEvents { get; set; }
        protected virtual BaseGameContext GameContext { get; set; }
        protected virtual BaseServer Server { get; set; }

        public abstract T Create<T>(int mask = -1) where T : BaseSnapEvent, new();
        public abstract void Clear();
        public abstract void OnSnapshot(int snappingClient);

        protected BaseEvents()
        {
            GameContext = Kernel.Get<BaseGameContext>();
            Server = Kernel.Get<BaseServer>();
        }
    }
}