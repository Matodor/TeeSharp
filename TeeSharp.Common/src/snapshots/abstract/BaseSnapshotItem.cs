using System.Runtime.InteropServices;
using TeeSharp.Common.Enums;

namespace TeeSharp.Common.Snapshots
{
    // TODO make snapshot item immutable
    [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
    public abstract class BaseSnapshotItem
    {
        public abstract SnapshotItems Type { get; }
    }
}