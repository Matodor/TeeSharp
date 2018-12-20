using System;
using System.Collections.Generic;
using TeeSharp.Common;

namespace TeeSharp.Server.Game
{
    public class Events : BaseEvents
    {
        protected override IList<EventInfo> EventInfos { get; set; }

        public Events()
        {
            MaxEvents = 128;
            EventInfos = new List<EventInfo>(128);
        }
        
        public override T Create<T>(int mask = -1)
        {
            if (EventInfos.Count == MaxEvents)
                return null;

            var info = new EventInfo
            {
                EventItem = new T(),
                Mask = mask
            };

            EventInfos.Add(info);

            return (T) info.EventItem;
        }

        public override void Clear()
        {
            EventInfos.Clear();
        }

        public override void OnSnapshot(int snappingClient)
        {
            for (var i = 0; i < EventInfos.Count; i++)
            {
                if (snappingClient == -1 || 
                    GameContext.MaskIsSet(EventInfos[i].Mask, snappingClient))
                {
                    if (snappingClient == -1 || 
                        MathHelper.Distance(GameContext.Players[snappingClient].ViewPos,
                            EventInfos[i].EventItem.Position) < 1500f)
                    {
                        Server.AddSnapItem(EventInfos[i].EventItem, i);
                    }
                }
            }
        }
    }
}