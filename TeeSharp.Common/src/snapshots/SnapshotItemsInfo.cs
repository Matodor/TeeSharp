using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using TeeSharp.Common.Enums;

namespace TeeSharp.Common.Snapshots
{
    public static class SnapshotItemsInfo
    {
        private static readonly Dictionary<Type, int> _sizes;

        static SnapshotItemsInfo()
        {
            _sizes = new Dictionary<Type, int>((int) SnapshotItems.NumItems);
        }

        public static int GetSize(Type type)
        {
            if (_sizes.ContainsKey(type))
                return _sizes[type];

            var size = Marshal.SizeOf(type);
            _sizes.Add(type, size);
            return size;
        }

        public static int GetSize<T>() where T : BaseSnapshotItem
        {
            return GetSize(typeof(T));
        }
    }
}