using System.Collections.Generic;
using TeeSharp.Common;

namespace TeeSharp.Server.Game
{
    public class Events : BaseEvents
    {
        protected override IList<EventInfo> EventInfos { get; set; }

        public override T Create<T>(Vector2 position, int mask = -1)
        {
            if (EventInfos.Count == MaxEvents)
                return null;

            var info = new EventInfo
            {
                EventItem = new T()
                {
                    X = (int) position.x,
                    Y = (int) position.y
                },
                Mask = mask
            };
            EventInfos.Add(info);

            return (T) info.EventItem;
        }

        public override void Init()
        {
            GameContext = Kernel.Get<BaseGameContext>();
            Server = Kernel.Get<BaseServer>();

            MaxEvents = 128;
            EventInfos = new List<EventInfo>(128);
        }

        public override void Clear()
        {
            EventInfos.Clear();
        }

        public override void OnSnapshot(int snappingClient)
        {
            for (var i = 0; i < EventInfos.Count; i++)
            {
                if (snappingClient == -1 || BaseGameContext.MaskIsSet(EventInfos[i].Mask, snappingClient))
                {
                    if (snappingClient == -1 || 
                        MathHelper.Distance(GameContext.Players[snappingClient].ViewPos,
                            new Vector2(EventInfos[i].EventItem.X, EventInfos[i].EventItem.Y)) < 1500f)
                    {
                        Server.SnapshotItem(EventInfos[i].EventItem, i);
                    }
                }
            }
        }
    }
}