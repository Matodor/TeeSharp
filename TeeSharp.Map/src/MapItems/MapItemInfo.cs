using System.Runtime.InteropServices;
using TeeSharp.Map.Abstract;

namespace TeeSharp.Map.MapItems;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct MapItemInfo : IDataFileItem
{
    public int ItemVersion;
    public int DataIndexAuthor;
    public int DataIndexVersion;
    public int DataIndexCredits;
    public int DataIndexLicense;
}
