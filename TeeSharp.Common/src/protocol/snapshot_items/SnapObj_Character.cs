using System.Runtime.InteropServices;
using TeeSharp.Common.Enums;

namespace TeeSharp.Common.Protocol
{
    [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
    public class SnapObj_Character : SnapObj_CharacterCore
    {
        public override SnapshotItems Type => SnapshotItems.Character;

        [MarshalAs(UnmanagedType.I4)] public int Health;
        [MarshalAs(UnmanagedType.I4)] public int Armor;
        [MarshalAs(UnmanagedType.I4)] public int AmmoCount;
        [MarshalAs(UnmanagedType.I4)] public Weapon Weapon;
        [MarshalAs(UnmanagedType.I4)] public Emote Emote;
        [MarshalAs(UnmanagedType.I4)] public int AttackTick;
        [MarshalAs(UnmanagedType.I4)] public CoreEventFlags TriggeredEvents;
    }
}