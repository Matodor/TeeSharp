using System;
using System.Runtime.InteropServices;

namespace Teecsharp
{
    class CHuffmanConstructNode
    {
        public ushort m_NodeId;
        public uint m_Frequency;
    }

    public class CHuffman
    {
        const int
            HUFFMAN_EOF_SYMBOL = 256,
            HUFFMAN_MAX_SYMBOLS = HUFFMAN_EOF_SYMBOL + 1,
            HUFFMAN_MAX_NODES = HUFFMAN_MAX_SYMBOLS * 2 - 1,
            HUFFMAN_LUTBITS = 10,
            HUFFMAN_LUTSIZE = (1 << HUFFMAN_LUTBITS),
            HUFFMAN_LUTMASK = (HUFFMAN_LUTSIZE - 1);

        class CNode
        {
            // symbol
            public int m_Bits;
            public uint m_NumBits;

            // don't use pointers for this. shorts are smaller so we can fit more data into the cache
            public ushort[] m_aLeafs = new ushort[2];

            // what the symbol represents
            public byte m_Symbol;
        };

        private CNode[] m_aNodes = new CNode[HUFFMAN_MAX_NODES];
        private CNode[] m_apDecodeLut = new CNode[HUFFMAN_LUTSIZE];
        private CNode m_pStartNode = new CNode();
        private int m_NumNodes;

        public void Init(uint[] pFrequencies)
        {
            int i;

            // make sure to cleanout every thing
            m_aNodes = new CNode[HUFFMAN_MAX_NODES];
            for (i = 0; i < HUFFMAN_MAX_NODES; i++)
                m_aNodes[i] = new CNode();

            m_apDecodeLut = new CNode[HUFFMAN_LUTSIZE];
            m_pStartNode = new CNode();
            m_NumNodes = 0;

            // construct the tree
            ConstructTree(pFrequencies);

            // build decode LUT
            for (i = 0; i < HUFFMAN_LUTSIZE; i++)
            {
                uint Bits = (uint)i;
                int k;
                CNode pNode = m_pStartNode;
                for (k = 0; k < HUFFMAN_LUTBITS; k++)
                {
                    pNode = m_aNodes[pNode.m_aLeafs[Bits & 1]];
                    Bits >>= 1;

                    if (pNode == null)
                        break;

                    if (pNode.m_NumBits != 0)
                    {
                        m_apDecodeLut[i] = pNode;
                        break;
                    }
                }

                if (k == HUFFMAN_LUTBITS)
                    m_apDecodeLut[i] = pNode;
            }

        }


        private void Setbits_r(CNode pNode, int Bits, uint Depth)
        {
            if (pNode.m_aLeafs[1] != 65535)
                Setbits_r(m_aNodes[pNode.m_aLeafs[1]], Bits | (1 << (int)Depth), Depth + 1);
            if (pNode.m_aLeafs[0] != 65535)
                Setbits_r(m_aNodes[pNode.m_aLeafs[0]], Bits, Depth + 1);

            if (pNode.m_NumBits != 0)
            {
                pNode.m_Bits = Bits;
                pNode.m_NumBits = Depth;
            }
        }

        static void BubbleSort(CHuffmanConstructNode[] ppList, int Size)
        {
            int Changed = 1;

            while (Changed != 0)
            {
                Changed = 0;
                for (int i = 0; i < Size - 1; i++)
                {
                    if (ppList[i].m_Frequency < ppList[i + 1].m_Frequency)
                    {
                        var pTemp = ppList[i];
                        ppList[i] = ppList[i + 1];
                        ppList[i + 1] = pTemp;
                        Changed = 1;
                    }
                }
                Size--;
            }
        }

        private void ConstructTree(uint[] pFrequencies)
        {
            CHuffmanConstructNode[] aNodesLeftStorage = new CHuffmanConstructNode[HUFFMAN_MAX_SYMBOLS];
            CHuffmanConstructNode[] apNodesLeft = new CHuffmanConstructNode[HUFFMAN_MAX_SYMBOLS];
            int NumNodesLeft = HUFFMAN_MAX_SYMBOLS;

            // add the symbols
            for (int i = 0; i < HUFFMAN_MAX_SYMBOLS; i++)
            {
                m_aNodes[i].m_NumBits = 4294967295;
                m_aNodes[i].m_Symbol = (byte)i;
                m_aNodes[i].m_aLeafs[0] = 0xffff;
                m_aNodes[i].m_aLeafs[1] = 0xffff;

                aNodesLeftStorage[i] = new CHuffmanConstructNode();
                if (i == HUFFMAN_EOF_SYMBOL)
                    aNodesLeftStorage[i].m_Frequency = 1;
                else
                    aNodesLeftStorage[i].m_Frequency = pFrequencies[i];
                aNodesLeftStorage[i].m_NodeId = (ushort)i;
                apNodesLeft[i] = aNodesLeftStorage[i];

            }

            m_NumNodes = HUFFMAN_MAX_SYMBOLS;

            // construct the table
            while (NumNodesLeft > 1)
            {
                // we can't rely on stdlib's qsort for this, it can generate different results on different implementations
                BubbleSort(apNodesLeft, NumNodesLeft);

                m_aNodes[m_NumNodes].m_NumBits = 0;
                m_aNodes[m_NumNodes].m_aLeafs[0] = apNodesLeft[NumNodesLeft - 1].m_NodeId;
                m_aNodes[m_NumNodes].m_aLeafs[1] = apNodesLeft[NumNodesLeft - 2].m_NodeId;
                apNodesLeft[NumNodesLeft - 2].m_NodeId = (ushort)m_NumNodes;
                apNodesLeft[NumNodesLeft - 2].m_Frequency = apNodesLeft[NumNodesLeft - 1].m_Frequency
                    + apNodesLeft[NumNodesLeft - 2].m_Frequency;

                m_NumNodes++;
                NumNodesLeft--;
            }

            // set start node
            m_pStartNode = m_aNodes[m_NumNodes - 1];
            // build symbol bits
            Setbits_r(m_pStartNode, 0, 0);
        }

        public int Compress(byte[] pInput, int pInputIndex, int InputSize, byte[] pOutput, int pOutputIndex, int OutputSize)
        {
            int pSrc = pInputIndex;
            int pSrcEnd = pSrc + InputSize;
            int pDst = pOutputIndex;
            int pDstEnd = pDst + OutputSize;

            // symbol variables
            int Bits = 0;
            int Bitcount = 0;

            // make sure that we have data that we want to compress
            if (InputSize != 0)
            {
                int Symbol = pInput[pSrc];
                pSrc += 1;

                while (pSrc != pSrcEnd)
                {
                    // {B} load the symbol
                    Bits |= m_aNodes[Symbol].m_Bits << Bitcount;
                    Bitcount += (int)m_aNodes[Symbol].m_NumBits;

                    // {C} fetch next symbol, this is done here because it will reduce dependency in the code
                    Symbol = pInput[pSrc];
                    pSrc += 1;

                    // {B} write the symbol loaded at
                    while (Bitcount >= 8)
                    {
                        pOutput[pDst] = (byte) (Bits & 0xff);
                        pDst += 1;
                        if (pDst == pDstEnd)
                            return -1;
                        Bits >>= 8;
                        Bitcount -= 8;
                    }
                }

                // write the last symbol loaded from {C} or {A} in the case of only 1 byte input buffer
                Bits |= m_aNodes[Symbol].m_Bits << Bitcount;
                Bitcount += (int)m_aNodes[Symbol].m_NumBits;
                while (Bitcount >= 8)
                {
                    pOutput[pDst] = (byte)(Bits & 0xff);
                    pDst += 1;
                    if (pDst == pDstEnd)
                        return -1;
                    Bits >>= 8;
                    Bitcount -= 8;
                }

            }

            // write EOF symbol
            Bits |= m_aNodes[HUFFMAN_EOF_SYMBOL].m_Bits << Bitcount;
            Bitcount += (int)m_aNodes[HUFFMAN_EOF_SYMBOL].m_NumBits;
            while (Bitcount >= 8)
            {
                pOutput[pDst] = (byte)(Bits & 0xff);
                pDst += 1;
                if (pDst == pDstEnd)
                    return -1;
                Bits >>= 8;
                Bitcount -= 8;
            }

            // write out the last bits
            pOutput[pDst] = (byte)Bits;
            pDst += 1;

            // return the size of the output
            return pDst - pOutputIndex;
        }

        public int Decompress(byte[] pInput, int pInputIndex, int InputSize, byte[] pOutput, int pOutputIndex, int OutputSize)
        {
            int pSrc = pInputIndex;
            int pSrcEnd = pSrc + InputSize;
            int pDst = pOutputIndex;
            int pDstEnd = pDst + OutputSize;

            // symbol variables
            int Bits = 0;
            int Bitcount = 0;

            CNode pEof = m_aNodes[HUFFMAN_EOF_SYMBOL];
            CNode pNode = null;

            while (true)
            {
                // {A} try to load a node now, this will reduce dependency at location {D}
                pNode = null;
                if (Bitcount >= HUFFMAN_LUTBITS)
                    pNode = m_apDecodeLut[Bits & HUFFMAN_LUTMASK];

                // {B} fill with new bits
                while (Bitcount < 24 && pSrc != pSrcEnd)
                {
                    Bits |= pInput[pSrc] << Bitcount;
                    pSrc += 1;
                    Bitcount += 8;
                }

                // {C} load symbol now if we didn't that earlier at location {A}
                if (pNode == null)
                    pNode = m_apDecodeLut[Bits & HUFFMAN_LUTMASK];

                if (pNode == null)
                    return -1;

                // {D} check if we hit a symbol already
                if (pNode.m_NumBits != 0)
                {
                    // remove the bits for that symbol
                    Bits >>= (int)pNode.m_NumBits;
                    Bitcount -= (int)pNode.m_NumBits;
                }
                else
                {
                    // remove the bits that the lut checked up for us
                    Bits >>= HUFFMAN_LUTBITS;
                    Bitcount -= HUFFMAN_LUTBITS;

                    // walk the tree bit by bit
                    while (true)
                    {
                        // traverse tree
                        pNode = m_aNodes[pNode.m_aLeafs[Bits & 1]];

                        // remove bit
                        Bitcount--;
                        Bits >>= 1;

                        // check if we hit a symbol
                        if (pNode.m_NumBits != 0)
                            break;

                        // no more bits, decoding error
                        if (Bitcount == 0)
                            return -1;
                    }
                }

                // check for eof
                if (pNode == pEof)
                    break;

                // output character
                if (pDst == pDstEnd)
                    return -1;

                pOutput[pDst] = pNode.m_Symbol;
                pDst += 1;
            }

            // return the size of the decompressed buffer
            return pDst - pOutputIndex;
        }
    }
}
