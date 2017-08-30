using System;
using System.Collections.Generic;
using System.Text;

namespace TeeSharp
{
    public class NetChunkHeader
    {
        public ChunkFlags Flags;
        public int Size;
        public int Sequence;

        public int Pack(byte[] data, int index)
        {
            data[index + 0] = (byte)((((int) Flags & 3) << 6) | ((Size >> 4) & 0x3f));
            data[index + 1] = (byte)(Size & 0xf);

            if ((Flags & ChunkFlags.VITAL) != 0)
            {
                data[index + 1] = (byte)(data[index + 1] | ((Sequence >> 2) & 0xf0));
                data[index + 2] = (byte)(Sequence & 0xff);

                return index + 3;
            }
            return index + 2;
        }

        public int Unpack(byte[] data, int index)
        {
            Flags = (ChunkFlags) ((data[index + 0] >> 6) & 3);
            Size = ((data[index + 0] & 0x3f) << 4) | (data[index + 1] & 0xf);
            Sequence = -1;

            if ((Flags & ChunkFlags.VITAL) != 0)
            {
                Sequence = ((data[index + 1] & 0xf0) << 2) | data[index + 2];
                return index + 3;
            }
            return index + 2;
        }
    }
}
