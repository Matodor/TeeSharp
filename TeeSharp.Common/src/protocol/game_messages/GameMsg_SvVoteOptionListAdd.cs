using TeeSharp.Common.Enums;

namespace TeeSharp.Common.Protocol
{
    public class GameMsg_SvVoteOptionListAdd : BaseGameMessage
    {
        public override GameMessages MsgId { get; } = GameMessages.SV_VOTEOPTIONLISTADD;

        public int NumOptions { get; set; }
        public string Description0 { get; set; }
        public string Description1 { get; set; }
        public string Description2 { get; set; }
        public string Description3 { get; set; }
        public string Description4 { get; set; }
        public string Description5 { get; set; }
        public string Description6 { get; set; }
        public string Description7 { get; set; }
        public string Description8 { get; set; }
        public string Description9 { get; set; }
        public string Description10 { get; set; }
        public string Description11 { get; set; }
        public string Description12 { get; set; }
        public string Description13 { get; set; }
        public string Description14 { get; set; }

        public override bool Pack(MsgPacker packer)
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