using System.Runtime.InteropServices;
using TeeSharp.Common.Enums;
using TeeSharp.Common.Snapshots;

namespace TeeSharp.Common.Protocol
{
    [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
    public class SnapshotEventDamage : BaseSnapshotEvent
    {
        public override SnapshotItems Type => SnapshotItems.EventDamage;

        public bool IsSelf
        {
            get => Self != 0;
            set => Self = value ? 1 : 0;
        }

        [MarshalAs(UnmanagedType.I4)] public int ClientId;
        [MarshalAs(UnmanagedType.I4)] public int Angle;
        [MarshalAs(UnmanagedType.I4)] public int HealthAmount;
        [MarshalAs(UnmanagedType.I4)] public int ArmorAmount;
        [MarshalAs(UnmanagedType.I4)] public int Self;
    }
}