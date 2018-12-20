using TeeSharp.Network.Enums;

namespace TeeSharp.Network
{
    public class ChunkHeader
    {
        public ChunkFlags Flags { get; set; }
        public int Size { get; set; }
        public int Sequence { get; set; }

        public int Pack(byte[] inputData, int inputOffset)
        {
            inputData[inputOffset + 0] =
                (byte) ((((int) Flags & 0x03) << 6) | ((Size >> 6) & 0x3F));
            inputData[inputOffset + 1] = (byte) (Size & 0x3F);

            if (Flags.HasFlag(ChunkFlags.Vital))
            {
                inputData[inputOffset + 1] |= (byte) ((Sequence >> 2) & 0xC0);
                inputData[inputOffset + 2] = (byte) (Sequence & 0xFF);
                return inputOffset + 3;
            }

            return inputOffset + 2;
        }

        public int Unpack(byte[] inputData, int inputOffset)
        {
            Sequence = -1;
            Flags = (ChunkFlags) ((inputData[inputOffset + 0] >> 6) & 0x03);
            Size = ((inputData[inputOffset + 0] & 0x3F) << 6) | 
                   ((inputData[inputOffset + 1] & 0x3F));

            if (Flags.HasFlag(ChunkFlags.Vital))
            {
                Sequence = ((inputData[inputOffset + 1] & 0xC0) << 2) |
                           ((inputData[inputOffset + 2]));
                return inputOffset + 3;
            }

            return inputOffset + 2;
        }
    }
}