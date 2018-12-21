namespace TeeSharp.Common
{
    public static class IntCompression
    {
        public static int Pack(byte[] data, int dataOffset, int value)
        {
            data[dataOffset] = (byte)((value >> 25) & 64);
            value = value ^ (value >> 31);

            data[dataOffset] = (byte)(data[dataOffset] | (value & 63));
            value >>= 6;

            if (value != 0)
            {
                data[dataOffset] = (byte)(data[dataOffset] | 128);

                while (true)
                {
                    dataOffset++;

                    data[dataOffset] = (byte)(value & 127);
                    value >>= 7;
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
            value = inputData[inputOffset] & 0b0011_1111;

            do
            {
                if ((inputData[inputOffset] & 0b1000_0000) == 0) break;
                inputOffset++;
                value |= (inputData[inputOffset] & 0b0111_1111) << 6;

                if ((inputData[inputOffset] & 0b1000_0000) == 0) break;
                inputOffset++;
                value |= (inputData[inputOffset] & 0b0111_1111) << (6 + 7);

                if ((inputData[inputOffset] & 0b1000_0000) == 0) break;
                inputOffset++;
                value |= (inputData[inputOffset] & 0b0111_1111) << (6 + 7 + 7);

                if ((inputData[inputOffset] & 0b1000_0000) == 0) break;
                inputOffset++;
                value |= (inputData[inputOffset] & 0b0111_1111) << (6 + 7 + 7 + 7);

            } while (false);

            inputOffset++;
            value ^= -sign;
            return inputOffset;
        }

        public static int Decompress(byte[] inputData, int inputOffset, 
            int inputSize, int[] outputData, int outputOffset)
        {
            var startOutputOffset = outputOffset;
            var end = inputOffset + inputSize;

            while (inputOffset < end)
            {
                inputOffset = Unpack(inputData, inputOffset, out var value);
                outputData[outputOffset++] = value;
            }

            return outputOffset - startOutputOffset; // Decompress size = count integer fields of snapshot items
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

            return outputOffset - startOutputOffset; // Compress size in bytes
        }
    }
}