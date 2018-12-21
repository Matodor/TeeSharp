using System.Runtime.InteropServices;
using TeeSharp.Common.Enums;

namespace TeeSharp.Common.Snapshots
{
    [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
    public abstract class BaseSnapObject
    {
        public abstract SnapshotItems Type { get; }
    }
}