using TeeSharp.Common.Enums;

namespace TeeSharp.Common.Protocol
{
    public class GameMsg_SvVoteOptionListAdd : BaseGameMessage
    {
        public override GameMessages MsgId { get; } = GameMessages.SV_VOTEOPTIONLISTADD;

        public int NumOptions;
        public string Description0;
        public string Description1;
        public string Description2;
        public string Description3;
        public string Description4;
        public string Description5;
        public string Description6;
        public string Description7;
        public string Description8;
        public string Description9;
        public string Description10;
        public string Description11;
        public string Description12;
        public string Description13;
        public string Description14;

        public override bool PackError(MsgPacker packer)
        {
            packer.AddInt(NumOptions);
            packer.AddString(Description0);
            packer.AddString(Description1);
            packer.AddString(Description2);
            packer.AddString(Description3);
            packer.AddString(Description4);
            packer.AddString(Description5);
            packer.AddString(Description6);
            packer.AddString(Description7);
            packer.AddString(Description8);
            packer.AddString(Description9);
            packer.AddString(Description10);
            packer.AddString(Description11);
            packer.AddString(Description12);
            packer.AddString(Description13);
            packer.AddString(Description14);
            return packer.Error;
        }
    }
}