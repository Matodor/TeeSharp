using System;

namespace Teecsharp
{
    public static class CVariableInt
    {
        public static int Pack(byte[] pDst, int pDstIndex, int i)
        {
            pDst[pDstIndex] = (byte) ((i >> 25) & 0x40);
            //Marshal.WriteByte(pDst, (byte)((i >> 25) & 0x40)); // set sign bit if i<0
            i = i ^ (i >> 31); // if(i<0) i = ~i

            pDst[pDstIndex] = (byte) (pDst[pDstIndex] | (i & 0x3F));
            //Marshal.WriteByte(pDst, (byte)(Marshal.ReadByte(pDst) | (i & 0x3F)));  // pack 6bit into dst
            i >>= 6; // discard 6 bits

            if (i != 0)
            {
                pDst[pDstIndex] = (byte)(pDst[pDstIndex] | 0x80);
                //Marshal.WriteByte(pDst, (byte)(Marshal.ReadByte(pDst) | 0x80)); // set extend bit

                while (true)
                {
                    pDstIndex += 1;

                    pDst[pDstIndex] = (byte)(i & 0x7F);
                    //Marshal.WriteByte(pDst, (byte)(i & 0x7F)); // pack 7bit
                    i >>= 7; // discard 7 bits
                    pDst[pDstIndex] = (byte)(pDst[pDstIndex] | (byte)(i != 0 ? 1 << 7 : 0 << 7));
                    //Marshal.WriteByte(pDst, (byte)(Marshal.ReadByte(pDst) | (byte)(i != 0 ? 1 << 7 : 0 << 7))); // set extend bit (may branch)
                    if (i == 0)
                        break;
                }
            }

            pDstIndex += 1;
            return pDstIndex;
        }

        public static int Unpack(byte[] pSrc, int pSrcIndex, out int pInOut)
        {
            int Sign = (pSrc[pSrcIndex] >> 6) & 1;
            pInOut = pSrc[pSrcIndex] & 0x3F;

            do
            {
                if ((pSrc[pSrcIndex] & 0x80) == 0) break;
                pSrcIndex += 1;
                pInOut |= (pSrc[pSrcIndex] & (0x7F)) << (6);

                if ((pSrc[pSrcIndex] & 0x80) == 0) break;
                pSrcIndex += 1;
                pInOut |= (pSrc[pSrcIndex] & (0x7F)) << (6 + 7);

                if ((pSrc[pSrcIndex] & 0x80) == 0) break;
                pSrcIndex += 1;
                pInOut |= (pSrc[pSrcIndex] & (0x7F)) << (6 + 7 + 7);

                if ((pSrc[pSrcIndex] & 0x80) == 0) break;
                pSrcIndex += 1;
                pInOut |= (pSrc[pSrcIndex] & (0x7F)) << (6 + 7 + 7 + 7);

            } while (false);

            pSrcIndex += 1;
            pInOut ^= -Sign; // if(sign) *i = ~(*i)
            return pSrcIndex;
        }

        public static void IntInByteArray(int value, byte[] pDst, int pDstIndex)
        {
            var intBytes = BitConverter.GetBytes(value);
            pDst[pDstIndex + 0] = intBytes[0];
            pDst[pDstIndex + 1] = intBytes[1];
            pDst[pDstIndex + 2] = intBytes[2];
            pDst[pDstIndex + 3] = intBytes[3];
        }

        public static long Decompress(byte[] pSrc, int pSrcIndex, int Size, byte[] pDst, int pDstIndex)
        {
            var pSrcIndex_ = pSrcIndex;
            var pDstIndex_ = pDstIndex;
            var pEnd = pSrcIndex_ + Size;

            while (pSrcIndex_ < pEnd)
            {
                int pOut;
                pSrcIndex_ = Unpack(pSrc, pSrcIndex_, out pOut);

                // TODO BitConverter.IsLittleEndian
                IntInByteArray(pOut, pDst, pDstIndex_);
                pDstIndex_ += sizeof(int);
            }

            return pDstIndex_ - pDstIndex;
        }

        public static long Compress(byte[] pSrc, int pSrcIndex, int Size, byte[] pDst, int pDstIndex)
        {
            var pSrcIndex_ = pSrcIndex;
            var pDstIndex_ = pDstIndex;
            Size = Size / sizeof(int);

            while (Size > 0)
            {
                // TODO BitConverter.IsLittleEndian
                var value = BitConverter.ToInt32(pSrc, pSrcIndex_);
                pDstIndex_ = Pack(pDst, pDstIndex_, value);
                pSrcIndex_ += sizeof(int);
                Size--;
            }

            return pDstIndex_ - pDstIndex;
        }
    }
}
