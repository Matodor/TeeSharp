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

        public static int GetSize<T>() where T : BaseSnapshotItem
        {
            if (_sizes.ContainsKey(typeof(T)))
                return _sizes[typeof(T)];

            var size = Marshal.SizeOf<T>();
            _sizes.Add(typeof(T), size);
            return size;
        }
    }
}