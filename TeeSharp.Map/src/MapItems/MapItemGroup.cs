using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using TeeSharp.Core.Extensions;
using TeeSharp.Map.Abstract;

namespace TeeSharp.Map.MapItems;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct MapItemGroup : IDataFileItem
{
    public const int CurrentVersion = 3;

    public unsafe Span<int> NameBuffer => new(Unsafe.AsPointer(ref _nameData1), 3);

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
    public int NumberOfLayers;

    private int _useClipping;

    public int ClipX;
    public int ClipY;
    public int ClipWidth;
    public int ClipHeight;

#pragma warning disable CS0169
    // ReSharper disable FieldCanBeMadeReadOnly.Local

    private int _nameData1;
    private int _nameData2;
    private int _nameData3;

    // ReSharper restore FieldCanBeMadeReadOnly.Local
#pragma warning restore CS0169
}
