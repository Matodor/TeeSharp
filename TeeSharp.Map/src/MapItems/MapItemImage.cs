using System.Runtime.InteropServices;

namespace TeeSharp.Map
{
    public struct MapItemImage : IDataFileItem
    {
        public const int CurrentVersion = 2;
        
        public bool IsExternal
        {
            get => _external == 1;
            set => _external = value ? 1 : 0;
        }
        
        public int ItemVersion;
        public int Width;
        public int Height;
        private int _external;
        public int DataIndexName;
        public int DateIndexImage;
        public int Format;
    }
}