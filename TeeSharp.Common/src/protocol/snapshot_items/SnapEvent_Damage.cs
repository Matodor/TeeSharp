using System.Runtime.InteropServices;
using TeeSharp.Common.Enums;
using TeeSharp.Common.Snapshots;

namespace TeeSharp.Common.Protocol
{
    [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
    public class SnapEvent_Damage : BaseSnapEvent
    {
        public override SnapshotItems Type => SnapshotItems.EventDamage;

        [MarshalAs(UnmanagedType.I4)] public int ClientID;
        [MarshalAs(UnmanagedType.I4)] public int Angle;
        [MarshalAs(UnmanagedType.I4)] public int HealthAmount;
        [MarshalAs(UnmanagedType.I4)] public int ArmorAmount;
        [MarshalAs(UnmanagedType.I4)] public int Self;
    }
}