using System.Diagnostics.CodeAnalysis;
using TeeSharp.Core.Extensions;
using Uuids;

namespace TeeSharp.Common.Protocol;

public static class UuidManager
{
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public static class DDNet
    {
        public static readonly Uuid RconType         = "rcon-type@ddnet.tw".CalculateUuid();
        public static readonly Uuid MapDetails       = "map-details@ddnet.tw".CalculateUuid();
        public static readonly Uuid Capabilities     = "capabilities@ddnet.tw".CalculateUuid();
        public static readonly Uuid ClientVersion    = "clientver@ddnet.tw".CalculateUuid();
        public static readonly Uuid Ping             = "ping@ddnet.tw".CalculateUuid();
        public static readonly Uuid Pong             = "pong@ddnet.tw".CalculateUuid();
        public static readonly Uuid ChecksumRequest  = "checksum-request@ddnet.tw".CalculateUuid();
        public static readonly Uuid ChecksumResponse = "checksum-response@ddnet.tw".CalculateUuid();
        public static readonly Uuid ChecksumError    = "checksum-error@ddnet.tw".CalculateUuid();
    }
}
