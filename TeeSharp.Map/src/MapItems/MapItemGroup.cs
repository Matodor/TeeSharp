using System;
using System.Runtime.CompilerServices;
using TeeSharp.Core.Extensions;

namespace TeeSharp.Map
{
    public struct MapItemGroup : IDataFileItem
    {
        public const int CurrentVersion = 3;

        public unsafe Span<int> NameBuffer 
            => new Span<int>(Unsafe.AsPointer(ref _nameData[0]), 3);

        public string Name
        {
            get => NameBuffer.GetString();
            set => NameBuffer.PutString(value);
        }

        public bool UseClipping
        {
            get => _useClipping == 1;
            set => _useClipping = value ? 1 : 0;
        }
        
        public int ItemVersion;
        public int OffsetX;
        public int OffsetY;
        public int ParallaxX;
        public int ParallaxY;
        public int StartLayer;
        public int LayersCount;
        private int _useClipping;
        public int ClipX;
        public int ClipY;
        public int ClipWidth;
        public int ClipHeight;
        private unsafe fixed int _nameData[3];
    }
}