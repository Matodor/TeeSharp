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

        /// <summary>
        /// Serialized size in bytes
        /// </summary>
        [MarshalAs(UnmanagedType.I4)]
        public readonly int Size;

        protected BaseSnapshotItem()
        {
            Size = SnapshotItemsInfo.GetSize(GetType());
        }

        public Span<int> ToArray()
        {
            var array = new int[Size / sizeof(int)];
            var handle = GCHandle.Alloc(array, GCHandleType.Pinned);
            var ptr = handle.AddrOfPinnedObject();

            Marshal.StructureToPtr(this, ptr, false);
            handle.Free();

            return array.AsSpan(1); // ignore Size fields
        }
    }
}