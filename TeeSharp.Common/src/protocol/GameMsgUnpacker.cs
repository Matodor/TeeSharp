using System;
using TeeSharp.Common.Enums;

namespace TeeSharp.Common.Protocol
{
    public class GameMsgUnpacker : BaseGameMsgUnpacker
    {
        public override bool Unpack(int msgId, Unpacker unpacker, 
            out BaseGameMessage msg, out string error)
        {
            var message = (GameMessages) msgId;
            switch (message)
            {
                case GameMessages.INVALID:
                    break;
                case GameMessages.SV_MOTD:
                    break;
                case GameMessages.SV_BROADCAST:
                    break;
                case GameMessages.SV_CHAT:
                    break;
                case GameMessages.SV_KILLMSG:
                    break;
                case GameMessages.SV_SOUNDGLOBAL:
                    break;
                case GameMessages.SV_TUNEPARAMS:
                    break;
                case GameMessages.SV_EXTRAPROJECTILE:
                    break;
                case GameMessages.SV_READYTOENTER:
                    break;
                case GameMessages.SV_WEAPONPICKUP:
                    break;
                case GameMessages.SV_EMOTICON:
                    break;
                case GameMessages.SV_VOTECLEAROPTIONS:
                    break;
                case GameMessages.SV_VOTEOPTIONLISTADD:
                    break;
                case GameMessages.SV_VOTEOPTIONADD:
                    break;
                case GameMessages.SV_VOTEOPTIONREMOVE:
                    break;
                case GameMessages.SV_VOTESET:
                    break;
                case GameMessages.SV_VOTESTATUS:
                    break;
                case GameMessages.CL_SAY:
                    break;
                case GameMessages.CL_SETTEAM:
                    break;
                case GameMessages.CL_SETSPECTATORMODE:
                    break;
                case GameMessages.CL_STARTINFO:
                    break;
                case GameMessages.CL_CHANGEINFO:
                    break;
                case GameMessages.CL_KILL:
                    break;
                case GameMessages.CL_EMOTICON:
                    break;
                case GameMessages.CL_VOTE:
                    break;
                case GameMessages.CL_CALLVOTE:
                    break;
                case GameMessages.CL_ISDDNET:
                    break;
                case GameMessages.NUM_MESSAGES:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            error = null;
            msg = null;
            return false;
        }

        public virtual GameMsg_SvMotd UnpackSvMotd()
        {
            return null;
        }
    }
}