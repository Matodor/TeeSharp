using TeeSharp.Map.MapItems;

namespace TeeSharp.Map
{
    public class LayerQuads : Layer
    {
        public int Image { get; set; }
        public Quad[] Quads { get; set; }
    }
}