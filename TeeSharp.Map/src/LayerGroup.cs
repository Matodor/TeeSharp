namespace TeeSharp.Map
{
    public class LayerGroup
    {
        public Layer[] Layers { get; set; }

        public int OffsetX { get; set; }
        public int OffsetY { get; set; }

        public int ParallaxX { get; set; }
        public int ParallaxY { get; set; }

        public int UseClipping { get; set; }
        public int ClipX { get; set; }
        public int ClipY { get; set; }
        public int ClipW { get; set; }
        public int ClipH { get; set; }

        public string Name { get; set; }
        public bool GameGroup { get; set; }
    }
}