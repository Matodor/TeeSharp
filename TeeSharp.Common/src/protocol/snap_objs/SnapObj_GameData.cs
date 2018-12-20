using System.Runtime.InteropServices;
using TeeSharp.Common.Enums;
using TeeSharp.Common.Snapshots;

namespace TeeSharp.Common.Protocol
{
    [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
    public class SnapObj_GameData : BaseSnapObject
    {
        public override SnapshotObjects Type => SnapshotObjects.GameData;

        [MarshalAs(UnmanagedType.I4)] public int GameStartTick;
        [MarshalAs(UnmanagedType.I4)] public GameStateFlags GameStateFlags;
        [MarshalAs(UnmanagedType.I4)] public int GameStateEndTick;
    }
}