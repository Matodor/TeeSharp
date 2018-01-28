namespace TeeSharp.Network
{
    public class Huffman
    {
        public const int
            EOF_SYMBOL = 256,
            MAX_SYMBOLS = EOF_SYMBOL + 1,
            MAX_NODES = MAX_SYMBOLS * 2 - 1,
            LUTBITS = 0b1010,
            LUTSIZE = 1 << LUTBITS,
            LUTMASK = LUTSIZE - 1;

        private class ConstructNode
        {
            public int NodeId;
            public int Frequency;
        }

        private class Node
        {
            public int Bits;
            public uint NumBits;
            public readonly int[] Leafs = new int[2];
            public byte Symbol;
        };

        private readonly Node[] _nodes;
        private readonly Node[] _decodeLut;
        private Node _startNode;
        private int _numNodes;

        public Huffman(int[] frequencies)
        {
            _nodes = new Node[MAX_NODES];
            for (var i = 0; i < MAX_NODES; i++)
                _nodes[i] = new Node();

            _decodeLut = new Node[LUTSIZE];
            _startNode = new Node();
            _numNodes = 0;

            ConstructTree(frequencies);

            for (var i = 0; i < LUTSIZE; i++)
            {
                var bits = i;
                var node = _startNode;

                for (var k = 0; k < LUTBITS; k++)
                {
                    node = _nodes[node.Leafs[bits & 1]];
                    bits >>= 1;

                    if (node == null)
                        break;

                    if (node.NumBits != 0)
                    {
                        _decodeLut[i] = node;
                        break;
                    }
                }

                _decodeLut[i] = node;
            }
        }
        
        private void SetBitsRecursively(Node node, int bits, int depth)
        {
            if (node.Leafs[1] != 0b1111_1111_1111_1111)    
                SetBitsRecursively(_nodes[node.Leafs[1]], bits | (1 << depth), depth + 1);
            if (node.Leafs[0] != 0b1111_1111_1111_1111)
                SetBitsRecursively(_nodes[node.Leafs[0]], bits, depth + 1);

            if (node.NumBits != 0)
            {
                node.Bits = bits;
                node.NumBits = (uint) depth;
            }
        }

        private static void BubbleSort(ConstructNode[] nodes, int size)
        {
            var changed = true;
            while (changed)
            {
                changed = false;
                for (var i = 0; i < size - 1; i++)
                {
                    if (nodes[i].Frequency < nodes[i + 1].Frequency)
                    {
                        var tmp = nodes[i];
                        nodes[i] = nodes[i + 1];
                        nodes[i + 1] = tmp;
                        changed = true;
                    }
                }

                size--;
            }
        }

        private void ConstructTree(int[] frequencies)
        {
            var nodesLeft = new ConstructNode[MAX_SYMBOLS];
            var numNodesLeft = MAX_SYMBOLS;

            for (var i = 0; i < MAX_SYMBOLS; i++)
            {
                _nodes[i].NumBits = 0b1111_1111_1111_1111_1111_1111_1111_1111;
                _nodes[i].Symbol = (byte) i;
                _nodes[i].Leafs[0] = 0b1111_1111_1111_1111;
                _nodes[i].Leafs[1] = 0b1111_1111_1111_1111;

                nodesLeft[i] =new ConstructNode
                {
                    Frequency = i == EOF_SYMBOL ? 1 : frequencies[i],
                    NodeId = i
                };
            }

            _numNodes = MAX_SYMBOLS;
            while (numNodesLeft > 1)
            {
                BubbleSort(nodesLeft, numNodesLeft);

                _nodes[_numNodes].NumBits = 0;
                _nodes[_numNodes].Leafs[0] = nodesLeft[numNodesLeft - 1].NodeId;
                _nodes[_numNodes].Leafs[1] = nodesLeft[numNodesLeft - 2].NodeId;

                nodesLeft[numNodesLeft - 2].NodeId = _numNodes;
                nodesLeft[numNodesLeft - 2].Frequency =
                    nodesLeft[numNodesLeft - 1].Frequency +
                    nodesLeft[numNodesLeft - 2].Frequency;

                _numNodes++;
                numNodesLeft--;
            }

            _startNode = _nodes[_numNodes - 1];
            SetBitsRecursively(_startNode, 0, 0);
        }

        public int Compress(byte[] source, int sourceOffset, int sourceSize,
            byte[] output, int outputOffset, int outputSize)
        {
            var sourceIndex = sourceOffset;
            var sourceEnd = sourceIndex + sourceSize;
            var outputIndex = outputOffset;
            var outputEnd = outputIndex + outputSize;
            
            var bits = 0;
            var bitCount = 0;

            if (sourceSize != 0)
            {
                int symbol = source[sourceIndex];
                sourceIndex += 1;

                while (sourceIndex != sourceEnd)
                {
                    bits |= _nodes[symbol].Bits << bitCount;
                    bitCount += (int)_nodes[symbol].NumBits;

                    symbol = source[sourceIndex];
                    sourceIndex += 1;

                    while (bitCount >= 8)
                    {
                        output[outputIndex] = (byte)(bits & 0xff);
                        outputIndex += 1;
                        if (outputIndex == outputEnd)
                            return -1;
                        bits >>= 8;
                        bitCount -= 8;
                    }
                }

                bits |= _nodes[symbol].Bits << bitCount;
                bitCount += (int)_nodes[symbol].NumBits;
                while (bitCount >= 8)
                {
                    output[outputIndex] = (byte)(bits & 0xff);
                    outputIndex += 1;

                    if (outputIndex == outputEnd)
                        return -1;

                    bits >>= 8;
                    bitCount -= 8;
                }

            }

            bits |= _nodes[EOF_SYMBOL].Bits << bitCount;
            bitCount += (int)_nodes[EOF_SYMBOL].NumBits;

            while (bitCount >= 8)
            {
                output[outputIndex] = (byte)(bits & 0b11111111);
                outputIndex += 1;

                if (outputIndex == outputEnd)
                    return -1;

                bits >>= 8;
                bitCount -= 8;
            }

            output[outputIndex] = (byte)bits;
            outputIndex += 1;

            return outputIndex - outputOffset;
        }

        public int Decompress(byte[] source, int sourceOffset, int sourceSize, 
            byte[] output, int outputOffset, int outputSize)
        {
            var sourceIndex = sourceOffset;
            var sourceEnd = sourceIndex + sourceSize;
            var outputIndex = outputOffset;
            var outputEnd = outputIndex + outputSize;

            var bits = 0;
            var bitCount = 0;

            while (true)
            {
                Node node = null;
                if (bitCount >= LUTBITS)
                    node = _decodeLut[bits & LUTMASK];

                while (bitCount < 24 && sourceIndex != sourceEnd)
                {
                    bits |= source[sourceIndex] << bitCount;
                    sourceIndex += 1;
                    bitCount += 8;
                }

                if (node == null)
                    node = _decodeLut[bits & LUTMASK];

                if (node == null)
                    return -1;

                if (node.NumBits != 0)
                {
                    bits >>= (int)node.NumBits;
                    bitCount -= (int)node.NumBits;
                }
                else
                {
                    bits >>= LUTBITS;
                    bitCount -= LUTBITS;

                    while (true)
                    {
                        node = _nodes[node.Leafs[bits & 1]];
                        bitCount--;
                        bits >>= 1;

                        if (node.NumBits != 0)
                            break;

                        if (bitCount == 0)
                            return -1;
                    }
                }

                if (node == _nodes[EOF_SYMBOL])
                    break;

                if (outputIndex == outputEnd)
                    return -1;

                output[outputIndex] = node.Symbol;
                outputIndex += 1;
            }

            return outputIndex - outputOffset;
        }
    }
}