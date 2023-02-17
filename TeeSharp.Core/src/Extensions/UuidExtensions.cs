using Uuids;

namespace TeeSharp.Core.Extensions;

public static class UuidExtensions
{
    public static string ToGuidString(this Uuid uuid)
    {
        return uuid.ToGuidStringLayout().ToString("D");
    }
}
