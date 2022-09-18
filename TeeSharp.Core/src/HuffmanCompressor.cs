using System;
using System.Collections.Generic;

namespace TeeSharp.Core;

public class HuffmanCompressor
{
    private class ConstructNode
    {
        internal ushort NodeId;
        internal uint Frequency;
    }

    private class Node
    {
        public int Bits;
        public uint NumBits;
        public readonly ushort[] Leafs = new ushort[2];
        public byte Symbol;
    }

    private const int
        EofSymbol = 256,
        MaxNodes = MaxSymbols * 2 - 1,
        MaxSymbols = EofSymbol + 1,
        LutBits = 10,
        LutSize = 1 << LutBits,
        LutMask = LutSize - 1;


    private readonly Node[] _nodes = new Node[MaxNodes];
    private readonly Node?[] _decodeLut = new Node[LutSize];
    private Node _startNode = new();
    private int _numNodes;

    public HuffmanCompressor(uint[] frequencies)
    {
        _numNodes = 0;

        for (var i = 0; i < MaxNodes; i++)
            _nodes[i] = new Node();

        ConstructTree(frequencies);

        for (var i = 0; i < LutSize; i++)
        {
            int k;
            var bits = (uint)i;
            var node = _startNode;

            for (k = 0; k < LutBits; k++)
            {
                node = _nodes[node.Leafs[bits & 1]];
                bits >>= 1;

                if (node.NumBits == 0)
                    continue;

                _decodeLut[i] = node;
                break;
            }

            if (k == LutBits)
                _decodeLut[i] = node;
        }
    }


    private void SetBitsRecursive(Node pNode, int bits, uint depth)
    {
        if (pNode.Leafs[1] != 65535)
            SetBitsRecursive(_nodes[pNode.Leafs[1]], bits | (1 << (int)depth), depth + 1);
        if (pNode.Leafs[0] != 65535)
            SetBitsRecursive(_nodes[pNode.Leafs[0]], bits, depth + 1);

        if (pNode.NumBits == 0)
            return;

        pNode.Bits = bits;
        pNode.NumBits = depth;
    }

    private static void SortNodes(IList<ConstructNode> nodes, int size)
    {
        var changed = 1;

        while (changed != 0)
        {
            changed = 0;
            for (var i = 0; i < size - 1; i++)
            {
                if (nodes[i].Frequency >= nodes[i + 1].Frequency)
                    continue;

                (nodes[i], nodes[i + 1]) = (nodes[i + 1], nodes[i]);
                changed = 1;
            }

            size--;
        }
    }

    private void ConstructTree(uint[] frequencies)
    {
        var nodesLeftStorage = new ConstructNode[MaxSymbols];
        var nodesLeft = new ConstructNode[MaxSymbols];
        var numNodesLeft = MaxSymbols;

        for (var i = 0; i < MaxSymbols; i++)
        {
            _nodes[i].NumBits = 4294967295;
            _nodes[i].Symbol = (byte)i;
            _nodes[i].Leafs[0] = 0xffff;
            _nodes[i].Leafs[1] = 0xffff;

            nodesLeftStorage[i] = new ConstructNode
            {
                Frequency = i == EofSymbol ? 1 : frequencies[i],
                NodeId = (ushort)i,
            };

            nodesLeft[i] = nodesLeftStorage[i];
        }

        _numNodes = MaxSymbols;

        while (numNodesLeft > 1)
        {
            SortNodes(nodesLeft, numNodesLeft);

            _nodes[_numNodes].NumBits = 0;
            _nodes[_numNodes].Leafs[0] = nodesLeft[numNodesLeft - 1].NodeId;
            _nodes[_numNodes].Leafs[1] = nodesLeft[numNodesLeft - 2].NodeId;

            nodesLeft[numNodesLeft - 2].NodeId = (ushort)_numNodes;
            nodesLeft[numNodesLeft - 2].Frequency = nodesLeft[numNodesLeft - 1].Frequency + nodesLeft[numNodesLeft - 2].Frequency;

            _numNodes++;
            numNodesLeft--;
        }

        _startNode = _nodes[_numNodes - 1];
        SetBitsRecursive(_startNode, 0, 0);
    }

    public int Compress(
        ReadOnlySpan<byte> input,
        Span<byte> output)
    {
        if (output.IsEmpty)
            return -1;

        var inputIndex = 0;
        var outputIndex = 0;

        var bits = 0;
        var bitCount = 0;

        if (!input.IsEmpty)
        {
            var symbol = input[inputIndex++];

            while (inputIndex < input.Length)
            {
                bits |= _nodes[symbol].Bits << bitCount;
                bitCount += (int)_nodes[symbol].NumBits;

                symbol = input[inputIndex++];

                while (bitCount >= 8)
                {
                    output[outputIndex++] = (byte)(bits & 0xff);

                    if (outputIndex == output.Length)
                        return -1;

                    bits >>= 8;
                    bitCount -= 8;
                }
            }

            bits |= _nodes[symbol].Bits << bitCount;
            bitCount += (int)_nodes[symbol].NumBits;

            while (bitCount >= 8)
            {
                output[outputIndex++] = (byte)(bits & 0xff);

                if (outputIndex == output.Length)
                    return -1;

                bits >>= 8;
                bitCount -= 8;
            }

        }

        bits |= _nodes[EofSymbol].Bits << bitCount;
        bitCount += (int)_nodes[EofSymbol].NumBits;

        while (bitCount >= 8)
        {
            output[outputIndex++] = (byte)(bits & 0xff);

            if (outputIndex == output.Length)
                return -1;

            bits >>= 8;
            bitCount -= 8;
        }

        output[outputIndex++] = (byte)bits;
        return outputIndex;
    }

    public int Decompress(
        ReadOnlySpan<byte> input,
        Span<byte> output)
    {
        var inputIndex = 0;
        var outputIndex = 0;

        var bits = 0;
        var bitCount = 0;
        var pEof = _nodes[EofSymbol];

        while (true)
        {
            Node? node = null;

            if (bitCount >= LutBits)
                node = _decodeLut[bits & LutMask];

            while (bitCount < 24 && inputIndex < input.Length)
            {
                bits |= input[inputIndex++] << bitCount;
                bitCount += 8;
            }

            node ??= _decodeLut[bits & LutMask];

            if (node == null)
                return -1;

            if (node.NumBits != 0)
            {
                bits >>= (int)node.NumBits;
                bitCount -= (int)node.NumBits;
            }
            else
            {
                bits >>= LutBits;
                bitCount -= LutBits;

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

            if (node == pEof)
                break;

            if (outputIndex == output.Length)
                return -1;

            output[outputIndex++] = node.Symbol;
        }

        return outputIndex;
    }
}
