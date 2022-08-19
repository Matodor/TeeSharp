using System.Runtime.InteropServices;
using TeeSharp.Map.Abstract;

namespace TeeSharp.Map.MapItems;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
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
    public int DataIndexImage;
    public int Format;
}
