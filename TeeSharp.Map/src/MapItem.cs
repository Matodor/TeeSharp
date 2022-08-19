using System.Runtime.InteropServices;
using TeeSharp.Map.Abstract;
using TeeSharp.Map.DataFileItems;

namespace TeeSharp.Map;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct MapItem<T> where T : struct, IDataFileItem
{
    public DataFileItem Info;
    public T Item;
}
