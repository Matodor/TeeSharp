using System;

namespace TeeSharp.MasterServer;

public enum MasterServerProtocolType
{
    SixIPv6,
    SixIPv4,
    SixupIPv6,
    SixupIPv4,
}

public static class MasterServerProtocolTypeExtensions
{
    public static string ToScheme(this MasterServerProtocolType type)
    {
        return type switch
        {
            MasterServerProtocolType.SixIPv6 => "tw-0.6+udp://",
            MasterServerProtocolType.SixIPv4 => "tw-0.6+udp://",
            MasterServerProtocolType.SixupIPv6 => "tw-0.7+udp://",
            MasterServerProtocolType.SixupIPv4 => "tw-0.7+udp://",
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };
    }

    public static string ToStringRepresentation(this MasterServerProtocolType type)
    {
        return type switch
        {
            MasterServerProtocolType.SixIPv6 => "tw0.6/ipv6",
            MasterServerProtocolType.SixIPv4 => "tw0.6/ipv4",
            MasterServerProtocolType.SixupIPv6 => "tw0.7/ipv6",
            MasterServerProtocolType.SixupIPv4 => "tw0.7/ipv4",
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };
    }
}
