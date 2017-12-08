using System;
using System.Collections.Generic;
using System.Text;

namespace TeeSharp
{
    public class Huffman
    {
        public const int
            HUFFMAN_EOF_SYMBOL = 256,
            HUFFMAN_MAX_SYMBOLS = HUFFMAN_EOF_SYMBOL + 1,
            HUFFMAN_MAX_NODES = HUFFMAN_MAX_SYMBOLS * 2 - 1,
            HUFFMAN_LUTBITS = 10,
            HUFFMAN_LUTSIZE = (1 << HUFFMAN_LUTBITS),
            HUFFMAN_LUTMASK = (HUFFMAN_LUTSIZE - 1);

        private class HuffmanConstructNode
        {
            public ushort NodeId;
            public uint Frequency;
        }

        private class Node
        {
            // symbol
            public int Bits;
            public uint NumBits;

            // don't use pointers for this. shorts are smaller so we can fit more data into the cache
            public readonly ushort[] Leafs = new ushort[2];

            // what the symbol represents
            public byte Symbol;
        };

        private Node[] Nodes = new Node[HUFFMAN_MAX_NODES];
        private Node[] DecodeLut = new Node[HUFFMAN_LUTSIZE];
        private Node StartNode = new Node();
        private int NumNodes;

        public void Init(uint[] frequencies)
        {
            int i;

            // make sure to cleanout every thing
            Nodes = new Node[HUFFMAN_MAX_NODES];
            for (i = 0; i < HUFFMAN_MAX_NODES; i++)
                Nodes[i] = new Node();

            DecodeLut = new Node[HUFFMAN_LUTSIZE];
            StartNode = new Node();
            NumNodes = 0;

            // construct the tree
            ConstructTree(frequencies);

            // build decode LUT
            for (i = 0; i < HUFFMAN_LUTSIZE; i++)
            {
                var bits = (uint)i;
                var node = StartNode;
                int k;

                for (k = 0; k < HUFFMAN_LUTBITS; k++)
                {
                    node = Nodes[node.Leafs[bits & 1]];
                    bits >>= 1;

                    if (node == null)
                        break;

                    if (node.NumBits != 0)
                    {
                        DecodeLut[i] = node;
                        break;
                    }
                }

                if (k == HUFFMAN_LUTBITS)
                    DecodeLut[i] = node;
            }

        }


        private void Setbits_r(Node node, int bits, uint depth)
        {
            if (node.Leafs[1] != 65535)
                Setbits_r(Nodes[node.Leafs[1]], bits | (1 << (int)depth), depth + 1);
            if (node.Leafs[0] != 65535)
                Setbits_r(Nodes[node.Leafs[0]], bits, depth + 1);

            if (node.NumBits != 0)
            {
                node.Bits = bits;
                node.NumBits = depth;
            }
        }

        private static void BubbleSort(HuffmanConstructNode[] list, int size)
        {
            var changed = 1;
            while (changed != 0)
            {
                changed = 0;
                for (var i = 0; i < size - 1; i++)
                {
                    if (list[i].Frequency < list[i + 1].Frequency)
                    {
                        var pTemp = list[i];
                        list[i] = list[i + 1];
                        list[i + 1] = pTemp;
                        changed = 1;
                    }
                }
                size--;
            }
        }

        private void ConstructTree(uint[] frequencies)
        {
            var nodesLeftStorage = new HuffmanConstructNode[HUFFMAN_MAX_SYMBOLS];
            var nodesLeft = new HuffmanConstructNode[HUFFMAN_MAX_SYMBOLS];
            var numNodesLeft = HUFFMAN_MAX_SYMBOLS;

            // add the symbols
            for (var i = 0; i < HUFFMAN_MAX_SYMBOLS; i++)
            {
                Nodes[i].NumBits = 4294967295;
                Nodes[i].Symbol = (byte)i;
                Nodes[i].Leafs[0] = 0xffff;
                Nodes[i].Leafs[1] = 0xffff;

                nodesLeftStorage[i] = new HuffmanConstructNode
                {
                    Frequency = i == HUFFMAN_EOF_SYMBOL ? 1 : frequencies[i],
                    NodeId = (ushort) i
                };
                nodesLeft[i] = nodesLeftStorage[i];

            }

            NumNodes = HUFFMAN_MAX_SYMBOLS;

            // construct the table
            while (numNodesLeft > 1)
            {
                // we can't rely on stdlib's qsort for this, it can generate different results on different implementations
                BubbleSort(nodesLeft, numNodesLeft);

                Nodes[NumNodes].NumBits = 0;
                Nodes[NumNodes].Leafs[0] = nodesLeft[numNodesLeft - 1].NodeId;
                Nodes[NumNodes].Leafs[1] = nodesLeft[numNodesLeft - 2].NodeId;
                nodesLeft[numNodesLeft - 2].NodeId = (ushort)NumNodes;
                nodesLeft[numNodesLeft - 2].Frequency = nodesLeft[numNodesLeft - 1].Frequency
                                                            + nodesLeft[numNodesLeft - 2].Frequency;

                NumNodes++;
                numNodesLeft--;
            }

            // set start node
            StartNode = Nodes[NumNodes - 1];
            // build symbol bits
            Setbits_r(StartNode, 0, 0);
        }

        public int Compress(byte[] input, int inputIndex, int inputSize, byte[] output, 
            int outputIndex, int outputSize)
        {
            var pSrc = inputIndex;
            var pSrcEnd = pSrc + inputSize;
            var pDst = outputIndex;
            var pDstEnd = pDst + outputSize;

            // symbol variables
            var Bits = 0;
            var Bitcount = 0;

            // make sure that we have data that we want to compress
            if (inputSize != 0)
            {
                int Symbol = input[pSrc];
                pSrc += 1;

                while (pSrc != pSrcEnd)
                {
                    // {B} load the symbol
                    Bits |= Nodes[Symbol].Bits << Bitcount;
                    Bitcount += (int)Nodes[Symbol].NumBits;

                    // {C} fetch next symbol, this is done here because it will reduce dependency in the code
                    Symbol = input[pSrc];
                    pSrc += 1;

                    // {B} write the symbol loaded at
                    while (Bitcount >= 8)
                    {
                        output[pDst] = (byte)(Bits & 0xff);
                        pDst += 1;
                        if (pDst == pDstEnd)
                            return -1;
                        Bits >>= 8;
                        Bitcount -= 8;
                    }
                }

                // write the last symbol loaded from {C} or {A} in the case of only 1 byte input buffer
                Bits |= Nodes[Symbol].Bits << Bitcount;
                Bitcount += (int)Nodes[Symbol].NumBits;
                while (Bitcount >= 8)
                {
                    output[pDst] = (byte)(Bits & 0xff);
                    pDst += 1;
                    if (pDst == pDstEnd)
                        return -1;
                    Bits >>= 8;
                    Bitcount -= 8;
                }

            }

            // write EOF symbol
            Bits |= Nodes[HUFFMAN_EOF_SYMBOL].Bits << Bitcount;
            Bitcount += (int)Nodes[HUFFMAN_EOF_SYMBOL].NumBits;
            while (Bitcount >= 8)
            {
                output[pDst] = (byte)(Bits & 0xff);
                pDst += 1;
                if (pDst == pDstEnd)
                    return -1;
                Bits >>= 8;
                Bitcount -= 8;
            }

            // write out the last bits
            output[pDst] = (byte)Bits;
            pDst += 1;

            // return the size of the output
            return pDst - outputIndex;
        }

        public int Decompress(byte[] input, int inputIndex, int inputSize, byte[] output, 
            int outputIndex, int outputSize)
        {
            var pSrc = inputIndex;
            var pSrcEnd = pSrc + inputSize;
            var pDst = outputIndex;
            var pDstEnd = pDst + outputSize;

            // symbol variables
            var Bits = 0;
            var Bitcount = 0;

            var pEof = Nodes[HUFFMAN_EOF_SYMBOL];

            while (true)
            {
                Node pNode = null;
                // {A} try to load a node now, this will reduce dependency at location {D}
                if (Bitcount >= HUFFMAN_LUTBITS)
                    pNode = DecodeLut[Bits & HUFFMAN_LUTMASK];

                // {B} fill with new bits
                while (Bitcount < 24 && pSrc != pSrcEnd)
                {
                    Bits |= input[pSrc] << Bitcount;
                    pSrc += 1;
                    Bitcount += 8;
                }

                // {C} load symbol now if we didn't that earlier at location {A}
                if (pNode == null)
                    pNode = DecodeLut[Bits & HUFFMAN_LUTMASK];

                if (pNode == null)
                    return -1;

                // {D} check if we hit a symbol already
                if (pNode.NumBits != 0)
                {
                    // remove the bits for that symbol
                    Bits >>= (int)pNode.NumBits;
                    Bitcount -= (int)pNode.NumBits;
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
                        pNode = Nodes[pNode.Leafs[Bits & 1]];

                        // remove bit
                        Bitcount--;
                        Bits >>= 1;

                        // check if we hit a symbol
                        if (pNode.NumBits != 0)
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

                output[pDst] = pNode.Symbol;
                pDst += 1;
            }

            // return the size of the decompressed buffer
            return pDst - outputIndex;
        }
    }
}
