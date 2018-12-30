using System;
using TeeSharp.Common.Enums;
using TeeSharp.Common.Game;

namespace TeeSharp.Server.Game
{
    public class TeeInfo
    {
        public SkinPartParams this[SkinPart part]
        {
            get
            {
                switch (part)
                {
                    case SkinPart.Body:
                        return Body;
                    case SkinPart.Marking:
                        return Marking;
                    case SkinPart.Decoration:
                        return Decoration;
                    case SkinPart.Hands:
                        return Hands;
                    case SkinPart.Feet:
                        return Feet;
                    case SkinPart.Eyes:
                        return Eyes;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(part), part, null);
                }
            }
        }

        public readonly SkinPartParams Body;
        public readonly SkinPartParams Marking;
        public readonly SkinPartParams Decoration;
        public readonly SkinPartParams Hands;
        public readonly SkinPartParams Feet;
        public readonly SkinPartParams Eyes;

        public TeeInfo()
        {
            Body = new SkinPartParams();
            Marking = new SkinPartParams();
            Decoration = new SkinPartParams();
            Hands = new SkinPartParams();
            Feet = new SkinPartParams();
            Eyes = new SkinPartParams();
        }
    }
}