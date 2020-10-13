using System.Runtime.InteropServices;

namespace TeeSharp.Map
{
    public struct MapItemImage : IDataFileItem
    {
        public int Version;
        public int Width;
        public int Height;
        public int External;
        public int DataIndexName;
        public int DateIndexImage;
    }
}