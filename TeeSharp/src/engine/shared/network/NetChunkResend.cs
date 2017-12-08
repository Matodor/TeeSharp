
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace TeeSharp
{
    public class NetChunkResend
    {
        public int Sequence;
        public ChunkFlags Flags;
        public int DataSize;
        public byte[] Data;
        public long FirstSendTime;
        public long LastSendTime;
    }
}
