using System.Runtime.InteropServices;
using TeeSharp.Common.Enums;
using TeeSharp.Common.Snapshots;

namespace TeeSharp.Common.Protocol
{
    [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
    public class SnapshotCharacterCore : BaseSnapshotItem
    {
        public override SnapshotItems Type => SnapshotItems.CharacterCore;

        [MarshalAs(UnmanagedType.I4)] public int Tick;
        [MarshalAs(UnmanagedType.I4)] public int X;
        [MarshalAs(UnmanagedType.I4)] public int Y;
        [MarshalAs(UnmanagedType.I4)] public int VelX;
        [MarshalAs(UnmanagedType.I4)] public int VelY;

        [MarshalAs(UnmanagedType.I4)] public int Angle;
        [MarshalAs(UnmanagedType.I4)] public int Direction;

        [MarshalAs(UnmanagedType.I4)] public int Jumped;
        [MarshalAs(UnmanagedType.I4)] public int HookedPlayer;
        [MarshalAs(UnmanagedType.I4)] public HookState HookState;
        [MarshalAs(UnmanagedType.I4)] public int HookTick;

        [MarshalAs(UnmanagedType.I4)] public int HookX;
        [MarshalAs(UnmanagedType.I4)] public int HookY;
        [MarshalAs(UnmanagedType.I4)] public int HookDx;
        [MarshalAs(UnmanagedType.I4)] public int HookDy;
    }
}