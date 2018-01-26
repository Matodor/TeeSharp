using TeeSharp.Core;
using TeeSharp.Map;

namespace TeeSharp.Common
{
    public abstract class BaseLayers : BaseInterface
    {
        public abstract LayerGroup GameGroup { get; protected set; }
        public abstract LayerGame GameLayer { get; protected set; }

        public abstract void Init(MapContainer map);
    }
}