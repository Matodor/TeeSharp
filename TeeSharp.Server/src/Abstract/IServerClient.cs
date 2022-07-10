using System.Diagnostics.CodeAnalysis;
using Uuids;

namespace TeeSharp.Server.Abstract;

public interface IServerClient
{
    int Id { get; }
    ServerClientState State { get; set; }
    Uuid? ConnectionUuid { get; set; }

    [SuppressMessage("ReSharper", "InconsistentNaming")]
    string? DDNetVersionString { get; set; }

    [SuppressMessage("ReSharper", "InconsistentNaming")]
    int? DDNetVersion { get; set; }
}
