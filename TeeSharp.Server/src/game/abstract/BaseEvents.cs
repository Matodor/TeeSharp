using System.Collections.Generic;
using TeeSharp.Common;
using TeeSharp.Common.Snapshots;
using TeeSharp.Core;

namespace TeeSharp.Server.Game
{
    public abstract class BaseEvents : BaseInterface
    {
        public struct EventInfo
        {
            public BaseSnapshotEvent EventItem;
            public int Mask;
        }

        protected abstract IList<EventInfo> EventInfos { get; set; }
        protected virtual int MaxEvents { get; set; }
        protected virtual BaseGameContext GameContext { get; set; }
        protected virtual BaseServer Server { get; set; }

        public abstract T Create<T>(Vector2 position, int mask = -1) where T : BaseSnapshotEvent, new();
        public abstract void Clear();
        public abstract void OnSnapshot(int snappingClient);

        protected BaseEvents()
        {
            GameContext = Kernel.Get<BaseGameContext>();
            Server = Kernel.Get<BaseServer>();
        }
    }
}