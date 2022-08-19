using System.Runtime.InteropServices;
using TeeSharp.Map.Abstract;

namespace TeeSharp.Map.MapItems;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct MapItemVersion : IDataFileItem
{
    public int Version;
}
