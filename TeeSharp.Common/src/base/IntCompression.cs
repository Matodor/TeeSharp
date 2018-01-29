using System;

namespace TeeSharp.Common
{
    public static class IntCompression
    {
        public static int Pack(byte[] data, int dataOffset, int value)
        {
            data[dataOffset] = (byte)((value >> 25) & 64);
            value = value ^ (value >> 31);

            data[dataOffset] = (byte)(data[dataOffset] | (value & 63));
            value >>= 6; // discard 6 bits

            if (value != 0)
            {
                data[dataOffset] = (byte)(data[dataOffset] | 128);

                while (true)
                {
                    dataOffset++;

                    data[dataOffset] = (byte)(value & 127);
                    value >>= 7; // discard 7 bits
                    data[dataOffset] = (byte)(data[dataOffset] | (byte)(value != 0 ? 128 : 0));

                    if (value == 0)
                        break;
                }
            }

            dataOffset++;
            return dataOffset;
        }

        public static int Unpack(byte[] inputData, int inputOffset, out int value)
        {
            var sign = (inputData[inputOffset] >> 6) & 1;
            value = inputData[inputOffset] & 63;

            do
            {
                if ((inputData[inputOffset] & 128) == 0) break;
                inputOffset++;
                value |= (inputData[inputOffset] & 127) << (6);

                if ((inputData[inputOffset] & 128) == 0) break;
                inputOffset++;
                value |= (inputData[inputOffset] & 127) << (6 + 7);

                if ((inputData[inputOffset] & 128) == 0) break;
                inputOffset++;
                value |= (inputData[inputOffset] & 127) << (6 + 7 + 7);

                if ((inputData[inputOffset] & 128) == 0) break;
                inputOffset++;
                value |= (inputData[inputOffset] & 127) << (6 + 7 + 7 + 7);

            } while (false);

            inputOffset++;
            value ^= -sign;
            return inputOffset;
        }

        /*public static void PasteInt(int value, byte[] destData, int destIndex)
        {
            var bytes = BitConverter.GetBytes(value);
            destData[destIndex + 0] = bytes[0];
            destData[destIndex + 1] = bytes[1];
            destData[destIndex + 2] = bytes[2];
            destData[destIndex + 3] = bytes[3];
        }*/

        public static int Decompress(byte[] inputData, int inputOffset, 
            int inputSize, int[] outputData, int outputOffset)
        {
            var startOutputOffset = outputOffset;
            var end = inputOffset + inputSize;

            while (inputOffset < end)
            {
                inputOffset = Unpack(inputData, inputOffset, out var value);
                outputData[outputOffset] = value;
                outputOffset++;
            }

            return outputOffset - startOutputOffset;
        }

        public static int Compress(int[] inputData, int inputOffset, 
            int inputSize, byte[] outputData, int outputOffset)
        {
            var startOutputOffset = outputOffset;

            for (var i = 0; i < inputSize; i++)
            {
                outputOffset = Pack(outputData, outputOffset, inputData[inputOffset]);
                inputOffset++;
            }

            return outputOffset - startOutputOffset;
        }
    }
}