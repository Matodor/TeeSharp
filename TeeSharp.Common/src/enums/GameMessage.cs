namespace TeeSharp.Common.Enums
{
    public enum GameMessage
    {
        Invalid = 0,

        ServerMotd,
        ServerBroadcast,
        ServerChat,
        ServerTeam,
        ServerKillMessage,
        ServerTuneParams,
        ServerExtraProjectile,
        ServerReadyToEnter,
        ServerWeaponPickup,
        ServerEmoticon,
        ServerVoteClearOptions,
        ServerVoteOptionListAdd,
        ServerVoteOptionAdd,
        ServerVoteOptionRemove,
        ServerVoteSet,
        ServerVoteStatus,
        ServerSettings,
        ServerClientInfo,
        ServerGameInfo,
        ServerClientDrop,
        ServerGameMessage,

        DemoClientEnter,
        DemoClientLeave,

        ClientSay,
        ClientSetTeam,
        ClientSetSpectatorMode,
        ClientStartInfo,
        ClientKill,
        ClientReadyChange,
        ClientEmoticon,
        ClientVote,
        ClientCallVote,

        NumMessages,
    }
}