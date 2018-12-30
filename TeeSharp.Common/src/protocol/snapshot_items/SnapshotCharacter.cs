using System;
using System.Runtime.InteropServices;
using TeeSharp.Common.Enums;

namespace TeeSharp.Common.Protocol
{
    [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
    public class SnapshotCharacter : SnapshotCharacterCore, IEquatable<SnapshotCharacter>
    {
        public override SnapshotItems Type => SnapshotItems.Character;

        [MarshalAs(UnmanagedType.I4)] public int Health;
        [MarshalAs(UnmanagedType.I4)] public int Armor;
        [MarshalAs(UnmanagedType.I4)] public int AmmoCount;
        [MarshalAs(UnmanagedType.I4)] public Weapon Weapon;
        [MarshalAs(UnmanagedType.I4)] public Emote Emote;
        [MarshalAs(UnmanagedType.I4)] public int AttackTick;
        [MarshalAs(UnmanagedType.I4)] public CoreEvents TriggeredEvents;

        public bool Equals(SnapshotCharacter other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return base.Equals(other) &&
                Health == other.Health && 
                Armor == other.Armor && 
                AmmoCount == other.AmmoCount && 
                Weapon == other.Weapon &&
                Emote == other.Emote && 
                AttackTick == other.AttackTick && 
                TriggeredEvents == other.TriggeredEvents;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((SnapshotCharacter) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = base.GetHashCode();
                hashCode = (hashCode * 397) ^ Health;
                hashCode = (hashCode * 397) ^ Armor;
                hashCode = (hashCode * 397) ^ AmmoCount;
                hashCode = (hashCode * 397) ^ (int) Weapon;
                hashCode = (hashCode * 397) ^ (int) Emote;
                hashCode = (hashCode * 397) ^ AttackTick;
                hashCode = (hashCode * 397) ^ (int) TriggeredEvents;
                return hashCode;
            }
        }
    }
}