using TeeSharp.Common.Enums;

namespace TeeSharp.Common.Protocol
{
    public class GameMsgUnpacker : BaseGameMsgUnpacker
    {
        protected const SanitizeType CC_TRIM = SanitizeType.SANITIZE_CC | SanitizeType.SKIP_START_WHITESPACES;

        public override bool Unpack(int msgId, Unpacker unpacker, 
            out BaseGameMessage msg, out string failedOn)
        {
            failedOn = string.Empty;
            var message = (GameMessages) msgId;

            switch (message)
            {
                case GameMessages.SV_MOTD:
                    msg = UnpackSvMotd(unpacker, ref failedOn);
                    break;

                case GameMessages.SV_BROADCAST:
                    msg = UnpackSvBroadcast(unpacker, ref failedOn);
                    break;

                case GameMessages.SV_CHAT:
                    msg = UnpackSvChat(unpacker, ref failedOn);
                    break;

                case GameMessages.SV_KILLMSG:
                    msg = UnpackSvKillMsg(unpacker, ref failedOn);
                    break;

                case GameMessages.SV_SOUNDGLOBAL:
                    msg = UnpackSvSoundGlobal(unpacker, ref failedOn);
                    break;

                case GameMessages.SV_TUNEPARAMS:
                    msg = UnpackSvTuneParams(unpacker, ref failedOn);
                    break;

                case GameMessages.SV_EXTRAPROJECTILE:
                    msg = UnpackSvExtraProjectile(unpacker, ref failedOn);
                    break;

                case GameMessages.SV_READYTOENTER:
                    msg = UnpackSvReadyToEnter(unpacker, ref failedOn);
                    break;

                case GameMessages.SV_WEAPONPICKUP:
                    msg = UnpackSvWeaponPickup(unpacker, ref failedOn);
                    break;

                case GameMessages.SV_EMOTICON:
                    msg = UnpackSvEmoticon(unpacker, ref failedOn);
                    break;

                case GameMessages.SV_VOTECLEAROPTIONS:
                    msg = UnpackSvVoteClearOptions(unpacker, ref failedOn);
                    break;

                case GameMessages.SV_VOTEOPTIONLISTADD:
                    msg = UnpackSvVoteOptionListAdd(unpacker, ref failedOn);
                    break;

                case GameMessages.SV_VOTEOPTIONADD:
                    msg = UnpackSvVoteOptionAdd(unpacker, ref failedOn);
                    break;

                case GameMessages.SV_VOTEOPTIONREMOVE:
                    msg = UnpackSvVoteOptionRemove(unpacker, ref failedOn);
                    break;

                case GameMessages.SV_VOTESET:
                    msg = UnpackSvVoteSet(unpacker, ref failedOn);
                    break;

                case GameMessages.SV_VOTESTATUS:
                    msg = UnpackSvVoteStatus(unpacker, ref failedOn);
                    break;

                case GameMessages.CL_SAY:
                    msg = UnpackClSay(unpacker, ref failedOn);
                    break;

                case GameMessages.CL_SETTEAM:
                    msg = UnpackClSetTeam(unpacker, ref failedOn);
                    break;

                case GameMessages.CL_SETSPECTATORMODE:
                    msg = UnpackClSetSpectatorMode(unpacker, ref failedOn);
                    break;

                case GameMessages.CL_STARTINFO:
                    msg = UnpackClStartInfo(unpacker, ref failedOn);
                    break;

                case GameMessages.CL_CHANGEINFO:
                    msg = UnpackClChangeInfo(unpacker, ref failedOn);
                    break;

                case GameMessages.CL_KILL:
                    msg = UnpackClKill(unpacker, ref failedOn);
                    break;

                case GameMessages.CL_EMOTICON:
                    msg = UnpackClEmoticon(unpacker, ref failedOn);
                    break;

                case GameMessages.CL_VOTE:
                    msg = UnpackClVote(unpacker, ref failedOn);
                    break;

                case GameMessages.CL_CALLVOTE:
                    msg = UnpackClCallVote(unpacker, ref failedOn);
                    break;

                case GameMessages.CL_ISDDNET:
                    msg = UnpackClIsDDNet(unpacker, ref failedOn);
                    break;

                default:
                    msg = null;
                    return false;
            }

            if (unpacker.Error)
            {
                failedOn = "unpack error";
                msg = null;
                return false;
            }

            if (!string.IsNullOrEmpty(failedOn))
            {
                msg = null;
                return false;
            }

            return true;
        }

        protected virtual GameMsg_SvMotd UnpackSvMotd(Unpacker unpacker, 
            ref string error)
        {
            return new GameMsg_SvMotd
            {
                Message = unpacker.GetString()
            };
        }

        protected virtual GameMsg_SvBroadcast UnpackSvBroadcast(Unpacker unpacker,
            ref string error)
        {
            return new GameMsg_SvBroadcast
            {
                Message = unpacker.GetString()
            };
        }

        protected virtual GameMsg_SvChat UnpackSvChat(Unpacker unpacker,
            ref string failedOn)
        {
            var msg = new GameMsg_SvChat
            {
                IsTeam = unpacker.GetInt() != 0,
                ClientId = unpacker.GetInt(),
                Message = unpacker.GetString()
            };

            if (msg.ClientId < -1)
                failedOn = "ClientId";

            return msg;
        }

        protected virtual GameMsg_SvKillMsg UnpackSvKillMsg(Unpacker unpacker,
            ref string failedOn)
        {
            var msg = new GameMsg_SvKillMsg
            {
                Killer = unpacker.GetInt(),
                Victim = unpacker.GetInt(),
                Weapon = (Weapon) unpacker.GetInt(),
                ModeSpecial = unpacker.GetInt()
            };

            if (msg.Killer < 0)
                failedOn = "Killer";
            if (msg.Victim < 0)
                failedOn = "Victim";
            if (msg.Weapon < Weapon.GAME)
                failedOn = "Weapon";

            return msg;
        }

        protected virtual GameMsg_SvSoundGlobal UnpackSvSoundGlobal(Unpacker unpacker,
            ref string failedOn)
        {
            var msg = new GameMsg_SvSoundGlobal
            {
                Sound = (Sounds) unpacker.GetInt()
            };

            if (msg.Sound < 0 || msg.Sound >= Sounds.NUM_SOUNDS)
                failedOn = "Sound";

            return msg;
        }

        protected virtual GameMsg_SvTuneParams UnpackSvTuneParams(Unpacker unpacker,
            ref string failedOn)
        {
            return new GameMsg_SvTuneParams();
        }

        protected virtual GameMsg_SvExtraProjectile UnpackSvExtraProjectile(Unpacker unpacker,
            ref string failedOn)
        {
            return new GameMsg_SvExtraProjectile();
        }

        protected virtual GameMsg_SvReadyToEnter UnpackSvReadyToEnter(Unpacker unpacker,
            ref string failedOn)
        {
            return new GameMsg_SvReadyToEnter();
        }

        protected virtual GameMsg_SvWeaponPickup UnpackSvWeaponPickup(Unpacker unpacker,
            ref string failedOn)
        {
            var msg = new GameMsg_SvWeaponPickup
            {
                Weapon = (Weapon) unpacker.GetInt()
            };

            if (msg.Weapon < 0 || msg.Weapon >= Weapon.NUM_WEAPONS)
                failedOn = "Weapon";

            return msg;
        }

        protected virtual GameMsg_SvEmoticon UnpackSvEmoticon(Unpacker unpacker,
            ref string failedOn)
        {
            var msg = new GameMsg_SvEmoticon
            {
                ClientId = unpacker.GetInt(),
                Emoticon = (Emoticons) unpacker.GetInt(),
            };

            if (msg.ClientId < 0)
                failedOn = "ClientId";
            if (msg.Emoticon < 0 || msg.Emoticon >= Emoticons.NUM_EMOTICONS)
                failedOn = "Emoticon";

            return msg;
        }

        protected virtual GameMsg_SvVoteClearOptions UnpackSvVoteClearOptions(
            Unpacker unpacker, ref string failedOn)
        {
            return new GameMsg_SvVoteClearOptions();
        }

        protected virtual GameMsg_SvVoteOptionListAdd UnpackSvVoteOptionListAdd(
            Unpacker unpacker, ref string failedOn)
        {
            var msg = new GameMsg_SvVoteOptionListAdd
            {
                NumOptions = unpacker.GetInt(),
                Description0 = unpacker.GetString(CC_TRIM),
                Description1 = unpacker.GetString(CC_TRIM),
                Description2 = unpacker.GetString(CC_TRIM),
                Description3 = unpacker.GetString(CC_TRIM),
                Description4 = unpacker.GetString(CC_TRIM),
                Description5 = unpacker.GetString(CC_TRIM),
                Description6 = unpacker.GetString(CC_TRIM),
                Description7 = unpacker.GetString(CC_TRIM),
                Description8 = unpacker.GetString(CC_TRIM),
                Description9 = unpacker.GetString(CC_TRIM),
                Description10 = unpacker.GetString(CC_TRIM),
                Description11 = unpacker.GetString(CC_TRIM),
                Description12 = unpacker.GetString(CC_TRIM),
                Description13 = unpacker.GetString(CC_TRIM),
                Description14 = unpacker.GetString(CC_TRIM),
            };

            if (msg.NumOptions < 1 || msg.NumOptions > 15)
                failedOn = "NumOptions";

            return msg;
        }

        protected virtual GameMsg_SvVoteOptionAdd UnpackSvVoteOptionAdd(
            Unpacker unpacker, ref string failedOn)
        {
            return new GameMsg_SvVoteOptionAdd
            {
                Description = unpacker.GetString(CC_TRIM)
            };
        }

        protected virtual GameMsg_SvVoteOptionRemove UnpackSvVoteOptionRemove(
            Unpacker unpacker, ref string failedOn)
        {
            return new GameMsg_SvVoteOptionRemove
            {
                Description = unpacker.GetString(CC_TRIM)
            };
        }

        protected virtual GameMsg_SvVoteSet UnpackSvVoteSet(Unpacker unpacker, 
            ref string failedOn)
        {
            var msg = new GameMsg_SvVoteSet
            {
                Timeout = unpacker.GetInt(),
                Description = unpacker.GetString(CC_TRIM),
                Reason = unpacker.GetString(CC_TRIM)
            };

            if (msg.Timeout < 0 || msg.Timeout > 60)
                failedOn = "Timeout";

            return msg;
        }

        protected virtual GameMsg_SvVoteStatus UnpackSvVoteStatus(Unpacker unpacker,
            ref string failedOn)
        {
            var msg = new GameMsg_SvVoteStatus
            {
                Yes = unpacker.GetInt(),
                No = unpacker.GetInt(),
                Pass = unpacker.GetInt(),
                Total = unpacker.GetInt(),
            };

            if (msg.Yes < 0)
                failedOn = "Yes";
            if (msg.No < 0)
                failedOn = "No";
            if (msg.Pass < 0)
                failedOn = "Pass";
            if (msg.Total < 0)
                failedOn = "Total";

            return msg;
        }

        protected virtual GameMsg_ClSay UnpackClSay(Unpacker unpacker,
            ref string failedOn)
        {
            return new GameMsg_ClSay
            {
                IsTeam = unpacker.GetInt() != 0,
                Message = unpacker.GetString()
            };
        }

        protected virtual GameMsg_ClSetTeam UnpackClSetTeam(Unpacker unpacker,
            ref string failedOn)
        {
            var msg = new GameMsg_ClSetTeam
            {
                Team = (Team) unpacker.GetInt()
            };

            if (msg.Team < Team.SPECTATORS || msg.Team > Team.BLUE)
                failedOn = "Team";

            return msg;
        }

        protected virtual GameMsg_ClSetSpectatorMode UnpackClSetSpectatorMode(
            Unpacker unpacker, ref string failedOn)
        {
            var msg = new GameMsg_ClSetSpectatorMode
            {
                SpectatorId = unpacker.GetInt()
            };

            if (msg.SpectatorId < -1)
                failedOn = "SpectatorId";

            return msg;
        }

        protected virtual GameMsg_ClStartInfo UnpackClStartInfo(
            Unpacker unpacker, ref string failedOn)
        {
            return new GameMsg_ClStartInfo
            {
                Name = unpacker.GetString(CC_TRIM),
                Clan = unpacker.GetString(CC_TRIM),
                Country = unpacker.GetInt(),
                Skin = unpacker.GetString(CC_TRIM),
                UseCustomColor = unpacker.GetInt() != 0,
                ColorBody = unpacker.GetInt(),
                ColorFeet = unpacker.GetInt()
            };
        }

        protected virtual GameMsg_ClChangeInfo UnpackClChangeInfo(
            Unpacker unpacker, ref string failedOn)
        {
            return new GameMsg_ClChangeInfo
            {
                Name = unpacker.GetString(CC_TRIM),
                Clan = unpacker.GetString(CC_TRIM),
                Country = unpacker.GetInt(),
                Skin = unpacker.GetString(CC_TRIM),
                UseCustomColor = unpacker.GetInt() != 0,
                ColorBody = unpacker.GetInt(),
                ColorFeet = unpacker.GetInt()
            };
        }

        protected virtual GameMsg_ClKill UnpackClKill(Unpacker unpacker, 
            ref string failedOn)
        {
            return new GameMsg_ClKill();
        }

        protected virtual GameMsg_ClEmoticon UnpackClEmoticon(Unpacker unpacker,
            ref string failedOn)
        {
            var msg = new GameMsg_ClEmoticon
            {
                Emoticon = (Emoticons) unpacker.GetInt()
            };

            if (msg.Emoticon < 0 || msg.Emoticon >= Emoticons.NUM_EMOTICONS)
                failedOn = "Emoticon";

            return msg;
        }

        protected virtual GameMsg_ClVote UnpackClVote(Unpacker unpacker,
            ref string failedOn)
        {
            var msg = new GameMsg_ClVote
            {
                Vote = unpacker.GetInt()
            };

            if (msg.Vote < -1 || msg.Vote > 1)
                failedOn = "Vote";

            return msg;
        }

        protected virtual GameMsg_ClCallVote UnpackClCallVote(Unpacker unpacker,
            ref string failedOn)
        {
            return new GameMsg_ClCallVote
            {
                Type = unpacker.GetString(CC_TRIM),
                Value = unpacker.GetString(CC_TRIM),
                Reason = unpacker.GetString(CC_TRIM)
            };
        }

        protected virtual GameMsg_ClIsDDNet UnpackClIsDDNet(Unpacker unpacker,
            ref string failedOn)
        {
            return new GameMsg_ClIsDDNet();
        }
    }
}