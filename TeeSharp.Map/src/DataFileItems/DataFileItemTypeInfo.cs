using System.Runtime.InteropServices;

namespace TeeSharp.Map.MapItems;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct DataFileItemTypeInfo
{
    public MapItemType Type;
    public int ItemsOffset;
    public int ItemsCount;
}
