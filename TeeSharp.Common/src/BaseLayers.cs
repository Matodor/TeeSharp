using TeeSharp.Core;
using TeeSharp.Map;
using TeeSharp.Map.MapItems;

namespace TeeSharp.Common
{
    public abstract class BaseLayers : BaseInterface
    {
        public abstract MapItemGroup GameGroup { get; protected set; }
        public abstract MapItemLayerTilemap GameLayer { get; protected set; }
        public abstract MapContainer Map { get; protected set; }

        protected abstract int GroupsStart { get; set; }
        protected abstract int GroupsNum { get; set; }
        protected abstract int LayersStart { get; set; }
        protected abstract int LayersNum { get; set; }

        public abstract void Init(MapContainer map);
        public abstract MapItemGroup GetGroup(int index);
        public abstract MapItemLayer GetLayer(int index);
    }
}