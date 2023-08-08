namespace TeeSharp.MasterServer;

public record MasterServerResponse(
    bool Successful,
    MasterServerResponseCode Code = MasterServerResponseCode.None
);
