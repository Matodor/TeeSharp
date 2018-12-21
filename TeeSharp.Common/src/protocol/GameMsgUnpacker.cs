using System;
using TeeSharp.Common.Enums;

namespace TeeSharp.Common.Protocol
{
    public class GameMsgUnpacker : BaseGameMsgUnpacker
    {
        public override bool UnpackMessage(GameMessage msg, 
            UnPacker unPacker, out BaseGameMessage value, out string failedOn)
        {
            switch (msg)
            {
                case GameMessage.ServerMotd:
                    value = new GameMsg_SvMotd();
                    break;

                case GameMessage.ServerBroadcast:
                    value = new GameMsg_SvBroadcast();
                    break;

                case GameMessage.ServerChat:
                    value = new GameMsg_SvChat();
                    break;
                case GameMessage.ServerTeam:
                    value = new GameMsg_SvTeam();
                    break;

                case GameMessage.ServerKillMessage:
                    value = new GameMsg_SvKillMsg();
                    break;

                case GameMessage.ServerTuneParams:
                    value = new GameMsg_SvTuneParams();
                    break;

                case GameMessage.ServerExtraProjectile:
                    value = new GameMsg_SvExtraProjectile();
                    break;

                case GameMessage.ServerReadyToEnter:
                    value = new GameMsg_SvReadyToEnter();
                    break;

                case GameMessage.ServerWeaponPickup:
                    value = new GameMsg_SvWeaponPickup();
                    break;

                case GameMessage.ServerEmoticon:
                    value = new GameMsg_SvEmoticon();
                    break;

                case GameMessage.ServerVoteClearOptions:
                    value = new GameMsg_SvVoteClearOptions();
                    break;

                case GameMessage.ServerVoteOptionListAdd:
                    value = new GameMsg_SvVoteOptionListAdd();
                    break;

                case GameMessage.ServerVoteOptionAdd:
                    value = new GameMsg_SvVoteOptionAdd();
                    break;

                case GameMessage.ServerVoteOptionRemove:
                    value = new GameMsg_SvVoteOptionRemove();
                    break;

                case GameMessage.ServerVoteSet:
                    value = new GameMsg_SvVoteSet();
                    break;

                case GameMessage.ServerVoteStatus:
                    value = new GameMsg_SvVoteStatus();
                    break;

                case GameMessage.ServerSettings:
                    value = new GameMsg_SvSettings();
                    break;

                case GameMessage.ServerClientInfo:
                    value = new GameMsg_SvClientInfo();
                    break;

                case GameMessage.ServerGameInfo:
                    value = new GameMsg_SvGameInfo();
                    break;

                case GameMessage.ServerClientDrop:
                    value = new GameMsg_SvClientDrop();
                    break;

                case GameMessage.ServerGameMessage:
                    value = new GameMsg_SvGameMsg();
                    break;

                case GameMessage.DemoClientEnter:
                    value = new GameMsg_DeClientEnter();
                    break;

                case GameMessage.DemoClientLeave:
                    value = new GameMsg_DeClientLeave();
                    break;

                case GameMessage.ClientSay:
                    value = new GameMsg_ClSay();
                    break;

                case GameMessage.ClientSetTeam:
                    value = new GameMsg_ClSetTeam();
                    break;

                case GameMessage.ClientSetSpectatorMode:
                    value = new GameMsg_ClSetSpectatorMode();
                    break;

                case GameMessage.ClientStartInfo:
                    value = new GameMsg_ClStartInfo();
                    break;

                case GameMessage.ClientKill:
                    value = new GameMsg_ClKill();
                    break;

                case GameMessage.ClientReadyChange:
                    value = new GameMsg_ClReadyChange();
                    break;

                case GameMessage.ClientEmoticon:
                    value = new GameMsg_ClEmoticon();
                    break;

                case GameMessage.ClientVote:
                    value = new GameMsg_ClVote();
                    break;

                case GameMessage.ClientCallVote:
                    value = new GameMsg_ClCallVote();
                    break;

                default:
                    value = null;
                    failedOn = null;
                    return false;
            }

            failedOn = null;
            if (!value.UnPackError(unPacker, ref failedOn))
                return failedOn == null;

            value = null;
            return false;
        }
    }
}