using TeeSharp.Map.MapItems;

namespace TeeSharp.Map
{
    public class LayerTiles : Layer
    {
        public bool GameTiles { get; set; }
        public int Image { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }

        public Color Color { get; set; }

        public int ColorEnv { get; set; }
        public int ColorEnvOffset { get; set; }

        public Tile[] Tiles { get; set; }
    }
}