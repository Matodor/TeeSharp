using TeeSharp.Network.Enums;

namespace TeeSharp.Network
{
    public class NetworkChunkHeader
    {
        public ChunkFlags Flags;
        public int Size;
        public int Sequence;

        public int Pack(byte[] inputData, int inputOffset)
        {
            inputData[inputOffset + 0] = (byte) ((((int) Flags & 0b11) << 6) | ((Size >> 4) & 0b111111));
            inputData[inputOffset + 1] = (byte) (Size & 0xf);

            if (Flags.HasFlag(ChunkFlags.VITAL))
            {
                inputData[inputOffset + 1] |= (byte) ((Sequence >> 2) & 0b1111_0000);
                inputData[inputOffset + 2] = (byte) (Sequence & 0b1111_1111);
                return inputOffset + 3;
            }

            return inputOffset + 2;
        }

        public int Unpack(byte[] inputData, int inputOffset)
        {
            Flags = (ChunkFlags) ((inputData[inputOffset + 0] >> 6) & 0b11);
            Size = ((inputData[inputOffset + 0] & 0b111111) << 4) | 
                   ((inputData[inputOffset + 1] & 0b1111));
            Sequence = -1;

            if (Flags.HasFlag(ChunkFlags.VITAL))
            {
                Sequence = ((inputData[inputOffset + 1] & 0b1111_0000) << 2) |
                           ((inputData[inputOffset + 2]));
                return inputOffset + 3;
            }

            return inputOffset + 2;
        }
    }
}