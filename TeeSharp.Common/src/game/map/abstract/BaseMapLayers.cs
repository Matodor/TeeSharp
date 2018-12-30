using TeeSharp.Core;
using TeeSharp.Map;
using TeeSharp.Map.MapItems;

namespace TeeSharp.Common
{
    public abstract class BaseMapLayers : BaseInterface
    {
        public virtual MapItemGroup GameGroup { get; protected set; }
        public virtual MapItemLayerTilemap GameLayer { get; protected set; }
        public virtual MapContainer Map { get; protected set; }

        protected virtual int GroupsStart { get; set; }
        protected virtual int GroupsNum { get; set; }
        protected virtual int LayersStart { get; set; }
        protected virtual int LayersNum { get; set; }

        public abstract void Init(MapContainer map);
        public abstract MapItemGroup GetGroup(int index);
        public abstract MapItemLayer GetLayer(int index);
    }
}