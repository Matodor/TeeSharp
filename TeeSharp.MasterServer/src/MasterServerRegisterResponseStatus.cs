namespace TeeSharp.MasterServer;

public enum MasterServerRegisterResponseStatus
{
    None = 0,
    Ok,
    NeedChallenge,
    NeedInfo,
}
