using System;
using NUnit.Framework;
using TeeSharp.Common.Extensions;
using TeeSharp.Common.Protocol;
using TeeSharp.Core;
using TeeSharp.Network;

namespace TeeSharp.Tests;

public class NetworkTests
{
    protected class PacketAccumulator
    {
        public NetworkPacketFlags Flags { get; set; } = NetworkPacketFlags.None;
        public int NumberOfMessages { get; set; }
        public int BufferSize { get; set; }
        public readonly byte[] Buffer = new byte[NetworkConstants.MaxPayload];

        public void Reset()
        {
            BufferSize = 0;
            NumberOfMessages = 0;
        }
    }

    [Test]
    public void TestQueueMessage()
    {
        var messageAccumulator = new PacketAccumulator();
        var sequence = 0;

        for (var i = 0; i < 3; i++)
        {
            var packer = new Packer();
            packer.AddProtocolMessage(ProtocolMessage.ServerMapChange);
            packer.AddString("FlatCity");
            packer.AddInteger(1734907049);
            packer.AddInteger(1772395);

            var data = packer.Buffer;
            var header = new NetworkMessageHeader(NetworkMessageHeaderFlags.Vital, data.Length, ++sequence);
            var buffer = messageAccumulator.Buffer.AsSpan(messageAccumulator.BufferSize);

            buffer = header.Pack(buffer);
            data.CopyTo(buffer);

            messageAccumulator.NumberOfMessages++;
            messageAccumulator.BufferSize +=
                messageAccumulator.Buffer.Length -
                messageAccumulator.BufferSize -
                buffer.Length +
                data.Length;
        }

        var expectMessageAccumulator = new PacketAccumulator
        {
            BufferSize = 66,
            NumberOfMessages = 3,
        };

        new byte[]
        {
            65,
            3,
            1,
            5,
            70,
            108,
            97,
            116,
            67,
            105,
            116,
            121,
            0,
            169,
            210,
            196,
            246,
            12,
            171,
            173,
            216,
            1,
            65,
            3,
            2,
            5,
            70,
            108,
            97,
            116,
            67,
            105,
            116,
            121,
            0,
            169,
            210,
            196,
            246,
            12,
            171,
            173,
            216,
            1,
            65,
            3,
            3,
            5,
            70,
            108,
            97,
            116,
            67,
            105,
            116,
            121,
            0,
            169,
            210,
            196,
            246,
            12,
            171,
            173,
            216,
            1,
        }.CopyTo(expectMessageAccumulator.Buffer.AsSpan());

        Assert.AreEqual(messageAccumulator.NumberOfMessages, expectMessageAccumulator.NumberOfMessages);
        Assert.AreEqual(messageAccumulator.BufferSize, expectMessageAccumulator.BufferSize);
        CollectionAssert.AreEqual(messageAccumulator.Buffer, expectMessageAccumulator.Buffer);
    }
}
