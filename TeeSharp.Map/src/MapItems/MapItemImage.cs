using System.Runtime.InteropServices;

namespace TeeSharp.Map
{
    public struct MapItemImage : IDataFileItem
    {
        public const int CurrentVersion = 2;
        
        public int ItemVersion;
        public int Width;
        public int Height;
        public int External;
        public int DataIndexName;
        public int DateIndexImage;
        public int Format;
    }
}