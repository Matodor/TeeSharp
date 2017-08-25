using System;

namespace TeeSharp
{
    public static class IntCompression
    {
        public static int Pack(byte[] destData, int destIndex, int intValue)
        {
            destData[destIndex] = (byte)((intValue >> 25) & 64);
            intValue = intValue ^ (intValue >> 31);

            destData[destIndex] = (byte)(destData[destIndex] | (intValue & 63));
            intValue >>= 6; // discard 6 bits

            if (intValue != 0)
            {
                destData[destIndex] = (byte)(destData[destIndex] | 128);

                while (true)
                {
                    ++destIndex;

                    destData[destIndex] = (byte)(intValue & 127);
                    intValue >>= 7; // discard 7 bits
                    destData[destIndex] = (byte)(destData[destIndex] | (byte)(intValue != 0 ? 128 : 0));

                    if (intValue == 0)
                        break;
                }
            }

            return ++destIndex;
        }

        public static int Unpack(byte[] sourceData, int sourceIndex, out int intValue)
        {
            var sign = (sourceData[sourceIndex] >> 6) & 1;
            intValue = sourceData[sourceIndex] & 63;

            do
            {
                if ((sourceData[sourceIndex] & 128) == 0) break;
                ++sourceIndex;
                intValue |= (sourceData[sourceIndex] & (127)) << (6);

                if ((sourceData[sourceIndex] & 128) == 0) break;
                ++sourceIndex;
                intValue |= (sourceData[sourceIndex] & (127)) << (6 + 7);

                if ((sourceData[sourceIndex] & 128) == 0) break;
                ++sourceIndex;
                intValue |= (sourceData[sourceIndex] & (127)) << (6 + 7 + 7);

                if ((sourceData[sourceIndex] & 128) == 0) break;
                ++sourceIndex;
                intValue |= (sourceData[sourceIndex] & (127)) << (6 + 7 + 7 + 7);

            } while (false);

            intValue ^= -sign;
            return ++sourceIndex;
        }

        public static void PasteInt(int value, byte[] destData, int destIndex)
        {
            var bytes = BitConverter.GetBytes(value);
            destData[destIndex + 0] = bytes[0];
            destData[destIndex + 1] = bytes[1];
            destData[destIndex + 2] = bytes[2];
            destData[destIndex + 3] = bytes[3];
        }

        public static long Decompress(byte[] sourceData, int sourceIndex, int size, byte[] destData, int destIndex)
        {
            var srcIndex = sourceIndex;
            var dstIndex = destIndex;
            var sourceEnd = sourceIndex + size;

            while (srcIndex < sourceEnd)
            {
                int pOut;
                srcIndex = Unpack(sourceData, srcIndex, out pOut);

                // TODO BitConverter.IsLittleEndian
                PasteInt(pOut, destData, dstIndex);
                dstIndex += sizeof(int);
            }

            return dstIndex - destIndex;
        }

        public static long Compress(byte[] sourceData, int sourceIndex, int size, byte[] destData, int destIndex)
        {
            var srcIndex = sourceIndex;
            var dstIndex = destIndex;

            size = size / sizeof(int);

            while (size > 0)
            {
                // TODO BitConverter.IsLittleEndian
                var value = BitConverter.ToInt32(sourceData, srcIndex);
                srcIndex += sizeof(int);
                dstIndex = Pack(destData, destIndex, value);
                --size;
            }

            return dstIndex - destIndex;
        }
    }
}
