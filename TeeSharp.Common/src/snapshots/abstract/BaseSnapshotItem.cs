using System;
using System.Runtime.InteropServices;
using TeeSharp.Common.Enums;

namespace TeeSharp.Common.Snapshots
{
    // TODO make snapshot item immutable
    [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
    public abstract class BaseSnapshotItem
    {
        public abstract SnapshotItems Type { get; }

        public static T FromArray<T>(int[] array) where T : BaseSnapshotItem, new()
        {
            var handle = GCHandle.Alloc(array, GCHandleType.Pinned);
            var ptr = handle.AddrOfPinnedObject();

            var obj = Marshal.PtrToStructure<T>(ptr);
            handle.Free();

            return obj;
        }

        public int[] ToArray()
        {
            var array = new int[SnapshotItemsInfo.GetSize(GetType()) / sizeof(int)];
            var handle = GCHandle.Alloc(array, GCHandleType.Pinned);
            var ptr = handle.AddrOfPinnedObject();

            Marshal.StructureToPtr(this, ptr, false);
            handle.Free();

            return array;
        }
    }
}