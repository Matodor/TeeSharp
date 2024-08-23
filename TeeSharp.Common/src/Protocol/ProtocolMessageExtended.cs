using System;
using System.Diagnostics.CodeAnalysis;
using System.Security.Cryptography;
using System.Text;
using Uuids;

namespace TeeSharp.Common;

[SuppressMessage("ReSharper", "MemberHidesStaticFromOuterClass")]
[SuppressMessage("ReSharper", "InconsistentNaming")]
[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
public partial class Protocol
{
    public class MessageExtended
    {
        public static class Common
        {
            public static readonly Uuid TeeworldsNamespace
                = Uuid.Parse("e05ddaaa-c4e6-4cfb-b642-5d48e80c0029");
        }

        public static class DDNet
        {
            public static class Messages
            {
                public static readonly Uuid ShowDistance
                    = CalculateUuid("show-distance@netmsg.ddnet.tw");

                public static readonly Uuid VoteOptionGroupStart
                    = CalculateUuid("sv-vote-option-group-start@netmsg.ddnet.org");

                public static readonly Uuid VoteOptionGroupEnd
                    = CalculateUuid("sv-vote-option-group-end@netmsg.ddnet.org");
            }

            public static class Objects
            {
                public static readonly Uuid GameInfo
                    = CalculateUuid("gameinfo@netobj.ddnet.tw");

                public static readonly Uuid Character
                    = CalculateUuid("character@netobj.ddnet.tw");

                public static readonly Uuid Player
                    = CalculateUuid("player@netobj.ddnet.tw");
            }

            public static class Events
            {
                public static readonly Uuid Finish
                    = CalculateUuid("finish@netevent.ddnet.org");

                public static readonly Uuid MapSoundWorld
                    = CalculateUuid("map-sound-world@netevent.ddnet.org");
            }

            public static readonly Uuid RconType
                = CalculateUuid("rcon-type@ddnet.tw");

            public static readonly Uuid MapDetails
                = CalculateUuid("map-details@ddnet.tw");

            public static readonly Uuid Capabilities
                = CalculateUuid("capabilities@ddnet.tw");

            public static readonly Uuid ClientVersion
                = CalculateUuid("clientver@ddnet.tw");

            public static readonly Uuid Ping
                = CalculateUuid("ping@ddnet.tw");

            public static readonly Uuid Pong
                = CalculateUuid("pong@ddnet.tw");

            public static readonly Uuid ChecksumRequest
                = CalculateUuid("checksum-request@ddnet.tw");

            public static readonly Uuid ChecksumResponse
                = CalculateUuid("checksum-response@ddnet.tw");

            public static readonly Uuid ChecksumError
                = CalculateUuid("checksum-error@ddnet.tw");
        }

        private const byte ResetVersionMask  = 0b_0000_1111;
        private const byte Version3Flag      = 0b_0011_0000;

        private const byte ResetReservedMask = 0b_0011_1111;
        private const byte ReservedFlag      = 0b_1000_0000;

        [SuppressMessage("ReSharper", "PossiblyImpureMethodCallOnReadonlyVariable")]
        public static Uuid CalculateUuid(string str)
        {
            var buffer = (Span<byte>)new byte[16 + Encoding.UTF8.GetMaxByteCount(str.Length)];

            if (Common.TeeworldsNamespace.TryWriteBytes(buffer.Slice(0, 16)) == false)
                throw new OutOfMemoryException();

            var strBytesCount = Encoding.UTF8.GetBytes(str, buffer.Slice(16));
            var hashData = MD5.HashData(buffer.Slice(0, 16 + strBytesCount));

            // set UUID version 3
            hashData[6] &= ResetVersionMask;
            hashData[6] |= Version3Flag;
            hashData[8] &= ResetReservedMask;
            hashData[8] |= ReservedFlag;

            return new Uuid(hashData);
        }
    }
}
